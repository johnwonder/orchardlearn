using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Features.Indexed;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Events;

namespace Orchard.Environment.ShellBuilders {

    public interface IShellContainerFactory {
        ILifetimeScope CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IShellContainerRegistrations _shellContainerRegistrations;

        public ShellContainerFactory(ILifetimeScope lifetimeScope, IShellContainerRegistrations shellContainerRegistrations) {
            _lifetimeScope = lifetimeScope;
            _shellContainerRegistrations = shellContainerRegistrations;
        }

        public ILifetimeScope CreateContainer(ShellSettings settings, ShellBlueprint blueprint) {
            //中间范围 
            //开始一个嵌套的生命周期，
            var intermediateScope = _lifetimeScope.BeginLifetimeScope(
                builder => {
                    foreach (var item in blueprint.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {
                        var registration = RegisterType(builder, item)
                            .Keyed<IModule>(item.Type)//用于检索组件
                            .InstancePerDependency();
                        //配置组件，每个独立成分或调用resolve()/ /获取一个新的，独特的实例
                        
                        foreach (var parameter in item.Parameters) {
                            registration = registration
                                .WithParameter(parameter.Name, parameter.Value)
                                .WithProperty(parameter.Name, parameter.Value);
                        }
                        //builder.Register(ctx => 2).OnActivated(e => e.Instance.)
                    }
                });

            //方法中的委托执行过后就执行Module的 Load AttachToComponentRegistration
            return intermediateScope.BeginLifetimeScope(
                "shell",
                builder => {
                    //AOP 
                    var dynamicProxyContext = new DynamicProxyContext();

                    builder.Register(ctx => dynamicProxyContext);
                    builder.Register(ctx => settings);//这里注册了ShellSettings
                    builder.Register(ctx => blueprint.Descriptor);//这里注册了ShellDescriptor
                    builder.Register(ctx => blueprint); //在CreateSessionFactory的时候用到 
                    //Autofac.Features.Indexed.IIndex<K,V>是autofac自动实现的一个关联类型。component可以使用IIndex<K,V>作为参数的构造函数从基于键的服务中选择需要的实现。
                    //http://www.cnblogs.com/wolegequ/archive/2012/06/03/2532605.html

                    var moduleIndex = intermediateScope.Resolve<IIndex<Type, IModule>>();
                    foreach (var item in blueprint.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {
                        builder.RegisterModule(moduleIndex[item.Type]);//根据IIndex来获取IModule并注册
                    }
                    //实现IDependency接口的依赖都可以使用动态代理 AOP
                    foreach (var item in blueprint.Dependencies.Where(t => typeof(IDependency).IsAssignableFrom(t.Type))) {
                        var registration = RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .InstancePerLifetimeScope();

                        foreach (var interfaceType in item.Type.GetInterfaces()
                            .Where(itf => typeof(IDependency).IsAssignableFrom(itf) 
                                      && !typeof(IEventHandler).IsAssignableFrom(itf))) {
                            registration = registration.As(interfaceType);//通过接口，比如IRouteProvider
                            if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                                registration = registration.InstancePerMatchingLifetimeScope("shell");
                            } 
                            else if (typeof(IUnitOfWorkDependency).IsAssignableFrom(interfaceType)) {
                                registration = registration.InstancePerMatchingLifetimeScope("work");
                            }
                            else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                                registration = registration.InstancePerDependency();
                            }
                        }

                        if (typeof(IEventHandler).IsAssignableFrom(item.Type)) {
                            registration = registration.As(typeof(IEventHandler));
                        }

                        foreach (var parameter in item.Parameters) {
                            registration = registration
                                .WithParameter(parameter.Name, parameter.Value)
                                .WithProperty(parameter.Name, parameter.Value);
                        }
                    }

                    foreach (var item in blueprint.Controllers) {
                        //这里很关键是通过AreaName和ControllerName注入 
                        var serviceKeyName = (item.AreaName + "/" + item.ControllerName).ToLowerInvariant();
                        var serviceKeyType = item.Type;
                        RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .Keyed<IController>(serviceKeyName)//这里供OrchardControllerFactory解析
                            .Keyed<IController>(serviceKeyType)
                            .WithMetadata("ControllerType", item.Type)
                            .InstancePerDependency()
                            .OnActivating(e => {
                                // necessary to inject custom filters dynamically
                                // see FilterResolvingActionInvoker
                                //需要动态注入filter
                                var controller = e.Instance as Controller;
                                if (controller != null)
                                    controller.ActionInvoker = (IActionInvoker)e.Context.ResolveService(new TypedService(typeof(IActionInvoker)));
                            });
                    }
                    //WebAPI
                    foreach (var item in blueprint.HttpControllers) {
                        var serviceKeyName = (item.AreaName + "/" + item.ControllerName).ToLowerInvariant();
                        var serviceKeyType = item.Type;
                        RegisterType(builder, item)
                            .EnableDynamicProxy(dynamicProxyContext)
                            .Keyed<IHttpController>(serviceKeyName)
                            .Keyed<IHttpController>(serviceKeyType)
                            .WithMetadata("ControllerType", item.Type)
                            .InstancePerDependency();
                    }

                    // Register code-only registrations specific to a shell
                    _shellContainerRegistrations.Registrations(builder);

                    var optionalShellConfig = HostingEnvironment.MapPath("~/Config/Sites.config");
                    if (File.Exists(optionalShellConfig))
                        builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, optionalShellConfig));

                    var optionalShellByNameConfig = HostingEnvironment.MapPath("~/Config/Sites." + settings.Name + ".config");
                    if (File.Exists(optionalShellByNameConfig))
                        builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReader.DefaultSectionName, optionalShellByNameConfig));
                });
        }

        /// <summary>
        /// DependencyBlueprint 继承自ShellBlueprintItem
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(ContainerBuilder builder, ShellBlueprintItem item) {
            return builder.RegisterType(item.Type)
                .WithProperty("Feature", item.Feature) 
                .WithMetadata("Feature", item.Feature);
        }
    }
}