using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace Orchard.Events {
    public class EventsRegistrationSource : IRegistrationSource {
        private readonly DefaultProxyBuilder _proxyBuilder;

        public EventsRegistrationSource() {
            _proxyBuilder = new DefaultProxyBuilder();
        }

        public bool IsAdapterForIndividualComponents {
            get { return false; }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor) {
            var serviceWithType = service as IServiceWithType;
            if (serviceWithType == null)
                yield break;

            var serviceType = serviceWithType.ServiceType;
            if (!serviceType.IsInterface || !typeof(IEventHandler).IsAssignableFrom(serviceType) || serviceType == typeof(IEventHandler))
                yield break;

            //interface proxy with target其实跟class proxy差不多，在创建代理对象时client指定接口，并且提供一个实现了该接口的对象作为真实对象，DP将创建这个接口的代理对象，对代理对象方法的调用经过拦截器处理之后，最终将调用真实对象相应的方法。与class proxy的不同之处在于，真实对象的方法不必是virtual类型也可以实现拦截
//interface proxy without target比较特殊，创建代理时只需要指定一个接口就可以，DP自动根据接口构造一个实现的类，作为代理对象的类型，但这个代理类只能用于拦截目的，无法像class proxy一样在拦截器中调用真实对象的处理方法。比如在提供了多个拦截器时，最后一个拦截器的接口方法中不能调用invocation.Proceed()方法，否则会抛异常（因为真实对象根本不存在，只有一个假的代理对象）
            var interfaceProxyType = _proxyBuilder.CreateInterfaceProxyTypeWithoutTarget(
                serviceType,
                new Type[0],
                ProxyGenerationOptions.Default);

            //关于代理
            //http://www.cnblogs.com/leiwei/p/3456695.html
            //http://www.cnblogs.com/RicCC/archive/2010/03/15/castle-dynamic-proxy.html
            //http://www.cnblogs.com/Tyrale/archive/2009/06/29/1512907.html

            var rb = RegistrationBuilder
                .ForDelegate((ctx, parameters) => {
                    var interceptors = new IInterceptor[] { new EventsInterceptor(ctx.Resolve<IEventBus>()) };
                    var args = new object[] { interceptors, null };
                    return Activator.CreateInstance(interfaceProxyType, args);
                })
                .As(service);

            yield return rb.CreateRegistration();
        }
    }
}