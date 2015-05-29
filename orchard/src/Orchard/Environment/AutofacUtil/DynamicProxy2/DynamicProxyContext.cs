using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace Orchard.Environment.AutofacUtil.DynamicProxy2 {
    public class DynamicProxyContext {
        const string ProxyContextKey = "Orchard.Environment.AutofacUtil.DynamicProxy2.DynamicProxyContext.ProxyContextKey";
        const string InterceptorServicesKey = "Orchard.Environment.AutofacUtil.DynamicProxy2.DynamicProxyContext.InterceptorServicesKey";

        readonly IProxyBuilder _proxyBuilder = new DefaultProxyBuilder();
        readonly IDictionary<Type, Type> _cache = new Dictionary<Type, Type>();

        /// <summary>
        /// Static method to resolve the context for a component registration. The context is set
        /// by using the registration builder extension method EnableDynamicProxy(context).
        /// </summary>
        public static DynamicProxyContext From(IComponentRegistration registration) {
            object value;
            //与组件相关的附加数据。中根据ProxyContextKey获取
            if (registration.Metadata.TryGetValue(ProxyContextKey, out value))
                return value as DynamicProxyContext;
            return null;
        }

        /// <summary>
        /// Called indirectly from the EnableDynamicProxy extension method.
        /// Modifies a registration to support dynamic interception if needed, and act as a normal type otherwise.
        /// 修改注册支持动态拦截，否则就是正常类型
        /// </summary>
        public void EnableDynamicProxy<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registrationBuilder)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData {
            
            // associate this context. used later by static DynamicProxyContext.From() method.
            //方便DynamicProxyContext.From调用
            registrationBuilder.WithMetadata(ProxyContextKey, this);

            // put a shim in place. this will return constructors for the proxy class if it interceptors have been added.
            //如果增加了拦截器那么就为代理类返回构造器
            registrationBuilder.ActivatorData.ConstructorFinder = new ConstructorFinderWrapper(
                registrationBuilder.ActivatorData.ConstructorFinder, this);

            // when component is being resolved, this even handler will place the array of appropriate interceptors as the first argument
            registrationBuilder.OnPreparing(e => {
                object value;
                //如果通过InterceptorServicesKey能获取到value
                if (e.Component.Metadata.TryGetValue(InterceptorServicesKey, out value)) {
                    var interceptorServices = (IEnumerable<Service>)value;
                    var interceptors = interceptorServices.Select(service => e.Context.ResolveService(service)).Cast<IInterceptor>().ToArray();
                    var parameter = new PositionalParameter(0, interceptors);//放在第一个参数
                    e.Parameters = new[] { parameter }.Concat(e.Parameters).ToArray();
                    //通过Concat连接参数
                }
            });
        }

        /// <summary>
        /// Called indirectly from the InterceptedBy extension method.
        /// 通过InterceptedBy扩展方法直接调用
        /// Adds services to the componenent's list of interceptors, activating the need for dynamic proxy
        /// </summary>
        public void AddInterceptorService(IComponentRegistration registration, Service service) {
            //DefaultProxyBuilder Castle 
            AddProxy(registration.Activator.LimitType);

            var interceptorServices = Enumerable.Empty<Service>();
            object value;
            if (registration.Metadata.TryGetValue(InterceptorServicesKey, out value)) {
                interceptorServices = (IEnumerable<Service>)value;
            }

            registration.Metadata[InterceptorServicesKey] = interceptorServices.Concat(new[] { service }).Distinct().ToArray();
        }


        /// <summary>
        /// Ensures that a proxy has been generated for the particular type in this context
        /// </summary>
        public void AddProxy(Type type) {
            Type proxyType;
            if (_cache.TryGetValue(type, out proxyType))
                return;

            lock (_cache) {
                if (_cache.TryGetValue(type, out proxyType))
                    return;

                _cache[type] = _proxyBuilder.CreateClassProxy(type, ProxyGenerationOptions.Default);
            }
        }

        /// <summary>
        /// Determines if a proxy has been generated for the given type, and returns it.
        /// </summary>
        public bool TryGetProxy(Type type, out Type proxyType) {
            return _cache.TryGetValue(type, out proxyType);
        }

    }
}
