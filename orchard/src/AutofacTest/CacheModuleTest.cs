using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace AutofacTest
{
    public class CacheModuleTest
    {
        public static void Main(string[] args)
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterModule(new CacheModule());
            builder.RegisterType<DefaultCacheHolder>().As<ICacheHolder>().SingleInstance();
            builder.RegisterType<DefaultProjectFileParser>().As<IProjectFileParser>().SingleInstance();


            IContainer container = builder.Build();

            IProjectFileParser cacheManager = container.Resolve<IProjectFileParser>();


           ILifetimeScope  lifeTime =  container.BeginLifetimeScope((b) => {

                b.RegisterType<MyController>().As<IController>();
            
            });

          IController controller =   lifeTime.Resolve<IController>();

        }
    }

    public  class DefaultProjectFileParser:IProjectFileParser
    {
        private ICacheManager _cacheManager;
        public DefaultProjectFileParser(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
    }

    public interface  IProjectFileParser
    {
    
    }

    public class DefaultCacheManager : ICacheManager
    {
        private readonly Type _component;
        private readonly ICacheHolder _cacheHolder;

        public DefaultCacheManager(Type component, ICacheHolder cacheHolder)
        {
            _component = component;
            _cacheHolder = cacheHolder;
        }
    }

    public interface ICacheManager
    {
    }

    public interface ICacheHolder
    {

    }


    public class DefaultCacheHolder : ICacheHolder
    {
    }

    public class CacheModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultCacheManager>()
                .As<ICacheManager>()
                .InstancePerDependency();//  PerDependency  如果新开一个LifetimeScope 那么会访问不到
        }

        protected override void AttachToComponentRegistration(Autofac.Core.IComponentRegistry componentRegistry, Autofac.Core.IComponentRegistration registration)
        {
            var needsCacheManager = registration.Activator.LimitType
                .GetConstructors()
                .Any(x => x.GetParameters()
                    .Any(xx => xx.ParameterType == typeof(ICacheManager)));

            if (needsCacheManager)
            {
                registration.Preparing += (sender, e) =>
                {
                    var parameter = new TypedParameter(
                        typeof(ICacheManager),
                        e.Context.Resolve<ICacheManager>(new TypedParameter(typeof(Type), registration.Activator.LimitType)));
                    e.Parameters = e.Parameters.Concat(new[] { parameter });
                };
            }
        }
    }


    public interface IController
    {

    }

    public class MyController:IController
    {
        private readonly ICacheManager _cacheManager;
        public MyController(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
    }
}
