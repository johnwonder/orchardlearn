﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Autofac.Core;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Logging;
using System.Diagnostics;

namespace Orchard.Environment.ShellBuilders {
    /// <summary>
    /// Service at the host level to transform the cachable descriptor into the loadable blueprint.
    /// </summary>
    public interface ICompositionStrategy {
        /// <summary>
        /// Using information from the IExtensionManager, transforms and populates all of the
        /// blueprint model the shell builders will need to correctly initialize a tenant IoC container.
        /// </summary>
        ShellBlueprint Compose(ShellSettings settings, ShellDescriptor descriptor);
    }

    public class CompositionStrategy : ICompositionStrategy {
        private readonly IExtensionManager _extensionManager;

        public CompositionStrategy(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        /// <summary>
        /// 返回ShellBlueprint
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public ShellBlueprint Compose(ShellSettings settings, ShellDescriptor descriptor) {
            Logger.Debug("Composing blueprint");

            //取扩展中Id为Shapes Orchard.Setup Orchard.jQuery 的扩展
            //Descriptor 中有三个Features Orchard.Setup Shapes Orchard.Jquery
            //ExtensionManager扩展方法 EnabledFeatures
            //比较descriptor的name 和 Id是否相同
            var enabledFeatures = _extensionManager.EnabledFeatures(descriptor);//获取扩展中的Features
            //LoadFeatures中 加载程序集 Settings为CoreExtensionLoader
            var features = _extensionManager.LoadFeatures(enabledFeatures);
            //Feature
            if (descriptor.Features.Any(feature => feature.Name == "Orchard.Framework"))
                features = features.Concat(BuiltinFeatures());//这里会导出MvcModule DataModule之类。

            //foreach (var item in features)
            //{
            //    //Logger.Information("Feature Name:"+item.Descriptor.)
            //}
            var excludedTypes = GetExcludedTypes(features);//Orchard.Data.Migration.DataMigrationNotificationProvider

            //DependencyBlueprint
            //ExtensionEntry 
            var modules = BuildBlueprint(features, IsModule, BuildModule, excludedTypes);
            var dependencies = BuildBlueprint(features, IsDependency, (t, f) => BuildDependency(t, f, descriptor), excludedTypes);//获取实现IDependency接口的
        
            var controllers = BuildBlueprint(features, IsController, BuildController, excludedTypes);
            var httpControllers = BuildBlueprint(features, IsHttpController, BuildController, excludedTypes);
            var records = BuildBlueprint(features, IsRecord, (t, f) => BuildRecord(t, f, settings), excludedTypes);

            var result = new ShellBlueprint {
                Settings = settings,
                Descriptor = descriptor,
                Dependencies = dependencies.Concat(modules).ToArray(),
                Controllers = controllers,
                HttpControllers = httpControllers,
                Records = records,
            };

            Logger.Debug("Done composing blueprint");
            return result;
        }

        private static IEnumerable<string> GetExcludedTypes(IEnumerable<Feature> features) {
            var excludedTypes = new HashSet<string>();

            // Identify replaced types
            foreach (Feature feature in features) {
                foreach (Type type in feature.ExportedTypes) {
                    foreach (OrchardSuppressDependencyAttribute replacedType in type.GetCustomAttributes(typeof(OrchardSuppressDependencyAttribute), false)) {
                        excludedTypes.Add(replacedType.FullName);
                    }
                }
            }

            return excludedTypes;
        }

        private static IEnumerable<Feature> BuiltinFeatures() {
            yield return new Feature {
                Descriptor = new FeatureDescriptor {
                    Id = "Orchard.Framework",
                    Extension = new ExtensionDescriptor {
                        Id = "Orchard.Framework"
                    }
                },
                ExportedTypes =
                    typeof(OrchardStarter).Assembly.GetExportedTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Except(new[] { typeof(DefaultOrchardHost) })
                    .ToArray()
            };
        }

        /// <summary>
        /// 最后还是通过selector linq返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="predicate"></param>
        /// <param name="selector"></param>
        /// <param name="excludedTypes"></param>
        /// <returns></returns>
        private static IEnumerable<T> BuildBlueprint<T>(
            IEnumerable<Feature> features,
            Func<Type, bool> predicate,
            Func<Type, Feature, T> selector,
            IEnumerable<string> excludedTypes ) {

            // Load types excluding the replaced types
            return features.SelectMany(
                feature => feature.ExportedTypes
                               .Where(predicate)
                               .Where(type => !excludedTypes.Contains(type.FullName))
                               .Select(type => selector(type, feature)))
                .ToArray();
        }

        private static bool IsModule(Type type) {
            return typeof(IModule).IsAssignableFrom(type);
        }

        private static DependencyBlueprint BuildModule(Type type, Feature feature) {
            return new DependencyBlueprint { Type = type, Feature = feature, Parameters = Enumerable.Empty<ShellParameter>() };
        }

        private static bool IsDependency(Type type) {
            //Debug.WriteLine("type:" + type.FullName);
            //这里会返回Orchard.Core.Settings.Descriptor.ShellDescriptorManager 等。。
            return typeof(IDependency).IsAssignableFrom(type);
        }

        private static DependencyBlueprint BuildDependency(Type type, Feature feature, ShellDescriptor descriptor) {
            return new DependencyBlueprint {
                Type = type,
                Feature = feature,
                Parameters = descriptor.Parameters.Where(x => x.Component == type.FullName).ToArray()
            };
        }

        private static bool IsController(Type type) {
            return typeof(IController).IsAssignableFrom(type);
        }

        private static bool IsHttpController(Type type) {
            return typeof(IHttpController).IsAssignableFrom(type);
        }

        private static ControllerBlueprint BuildController(Type type, Feature feature) {
            var areaName = feature.Descriptor.Extension.Id;

            var controllerName = type.Name;
            if (controllerName.EndsWith("Controller"))
                controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

            return new ControllerBlueprint {
                Type = type,
                Feature = feature,
                AreaName = areaName,//AreaName在这里加上 便于后面 ControllFactory中去寻找
                ControllerName = controllerName,
            };
        }
        /// <summary>
        /// 找到需要创建映射关系的实体类
        /// 命名空间判断 Models 或者 Records结尾
        /// 并且有virtual的 Id属性 
        /// 继承IContent或者ContentPartRecord接口的
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsRecord(Type type) {
            return ((type.Namespace ?? "").EndsWith(".Models") || (type.Namespace ?? "").EndsWith(".Records")) &&
                   type.GetProperty("Id") != null &&
                   (type.GetProperty("Id").GetAccessors() ?? Enumerable.Empty<MethodInfo>()).All(x => x.IsVirtual) &&
                   !type.IsSealed &&
                   !type.IsAbstract &&
                   (!typeof(IContent).IsAssignableFrom(type) || typeof(ContentPartRecord).IsAssignableFrom(type));
        }

        /// <summary>
        /// 相当于返回表名列表 带上ShellSettings的Name
        /// 用于在AbstractDataServicesProvider 中 Mapping
        /// </summary>
        /// <param name="type"></param>
        /// <param name="feature"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static RecordBlueprint BuildRecord(Type type, Feature feature, ShellSettings settings) {
            var extensionDescriptor = feature.Descriptor.Extension;
            var extensionName = extensionDescriptor.Id.Replace('.', '_');
            
            var dataTablePrefix = "";
            if (!string.IsNullOrEmpty(settings.DataTablePrefix))
                dataTablePrefix = settings.DataTablePrefix + "_";

            return new RecordBlueprint {
                Type = type,
                Feature = feature,
                TableName = dataTablePrefix + extensionName + '_' + type.Name,
            };
        }
    }
}