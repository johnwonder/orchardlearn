using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Interceptor;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Autofac;

namespace ConsoleDynamicProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            //ProxyContextHasBeenProxied();

            InterceptorAddedToContextFromModules();
            Console.ReadLine();
        }

        static void ProxyContextHasBeenProxied()
        {
            var context = new DynamicProxyContext();
            context.AddProxy(typeof(SimpleComponent));

            Type proxyType;
            context.TryGetProxy(typeof(SimpleComponent), out proxyType);
            Console.WriteLine(proxyType.FullName);
        }

        static void InterceptorAddedToContextFromModules()
        {
            var context = new DynamicProxyContext();
            var builder = new ContainerBuilder();
            builder.RegisterType<SimpleComponent>().EnableDynamicProxy(context);
            builder.RegisterModule(new SimpleInterceptorModule());

            IContainer container =  builder.Build();

            SimpleComponent simpleComponent = container.Resolve<SimpleComponent>();

            Console.WriteLine(simpleComponent.SimpleMethod());


        }
    }

    public class SimpleComponent
    {
        public virtual string SimpleMethod()
        {
            return "default return value";
        }
    }

    public class SimpleInterceptorModule:Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SimpleInterceptor>();
            base.Load(builder);
        }

        protected override void AttachToComponentRegistration(Autofac.Core.IComponentRegistry componentRegistry, Autofac.Core.IComponentRegistration registration)
        {
            if (DynamicProxyContext.From(registration) != null)
                registration.InterceptedBy<SimpleInterceptor>();
        }
    }

    public class SimpleInterceptor:IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name == "SimpleMethod")
            {
                invocation.Proceed();//调用父类函数 //返回default return value
              //  invocation.ReturnValue = "different return value";
            }
            else
            {
                invocation.Proceed();
            }
        }
    }

    class CallingLogInterceptor:IInterceptor
    {
        private int _indent = 0;

        private void PreProceed(IInvocation invocation)
        {
            if (this._indent > 0)
                Console.Write(" ".PadRight(this._indent * 4,' '));
            this._indent++;
            Console.Write("Intercepting: " + invocation.Method.Name + "(");
            if (invocation.Arguments != null && invocation.Arguments.Length > 0)
            {
                for (int i = 0; i < invocation.Arguments.Length; i++)
                {
                    if (i != 0) Console.Write(", ");
                    Console.Write(invocation.Arguments[i] == null
                        ? "null"
                        : invocation.Arguments[i].GetType() == typeof(string)
                        ? "\"" + invocation.Arguments[i].ToString() + "\""
                        : invocation.Arguments[i].ToString());
                }
              
            }
            Console.Write(")");
        }

        private void PostProceed(IInvocation invocation)
        {
            this._indent--;
        }

        public void Intercept(IInvocation invocation)
        {
            this.PreProceed(invocation);
            invocation.Proceed();
            this.PostProceed(invocation);
        }
    }
}