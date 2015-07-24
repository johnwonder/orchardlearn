using System;
using System.Linq;
using Autofac;

namespace Orchard.Caching {
    public class CacheModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<DefaultCacheManager>()
                .As<ICacheManager>()
                .InstancePerDependency();  //Module中用 PerDependency 导致 SetupMode中必须注册CacheModule SingleInstance 就不用
        }

        protected override void AttachToComponentRegistration(Autofac.Core.IComponentRegistry componentRegistry, Autofac.Core.IComponentRegistration registration) {
            var needsCacheManager = registration.Activator.LimitType
                .GetConstructors()
                .Any(x => x.GetParameters()
                    .Any(xx => xx.ParameterType == typeof(ICacheManager)));

            if (needsCacheManager) {
                registration.Preparing += (sender, e) => {
                    var parameter = new TypedParameter(
                        typeof(ICacheManager),
                        e.Context.Resolve<ICacheManager>(new TypedParameter(typeof(Type), registration.Activator.LimitType)));//哦 放在这里是要把Component加上去
                    e.Parameters = e.Parameters.Concat(new[] { parameter });
                };
            }
        }
    }
}
