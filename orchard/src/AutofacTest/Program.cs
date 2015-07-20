using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Core;
using AutofacTest;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main1(string[] args)
        {
            TestLifeTimeScope.EnumerablesFromDifferentLifetimeScope();
           
            //AutofacMeta.TestAutofacMeta();

            Console.ReadLine();
        }

        static void TestLifeTimeScopeWithTag()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<ShellContainerFactory>().As<IShellContainerFactory>().SingleInstance();


            IContainer container = builder.Build();

            IShellContainerFactory factory = container.Resolve<IShellContainerFactory>();

            ILifetimeScope lifeTime = factory.CreateContainer();

            var workLifetime = lifeTime.BeginLifetimeScope("work");
            //var workLifetime = lifeTime.BeginLifetimeScope();
            //这里不加work也会出错

            WorkContext workContext = workLifetime.Resolve<WorkContext>();

            IWorker worker = workContext.Resolve<IWorker>();
            worker.DoWork();
            //ILifetimeScope scope = builder.be
        }
    }

    public class WorkContextModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<Worker>()
              .As<IWorker>()
              .InstancePerMatchingLifetimeScope("shell");

            //先调用WorkContextImplementation的构造函数传入IComponentContext
            builder.Register(ctx => new WorkContextImplementation(ctx.Resolve<IComponentContext>()))
              .As<WorkContext>()
              .InstancePerMatchingLifetimeScope("work");
        }
    }

    public interface IShellContainerFactory
    {
        ILifetimeScope CreateContainer();
    }

    public class ShellContainerFactory:IShellContainerFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public ShellContainerFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public ILifetimeScope CreateContainer()
        {
            var intermediateScope = _lifetimeScope.BeginLifetimeScope(
                builder => {
                    builder.RegisterModule(new WorkContextModule());
                });

            return intermediateScope.BeginLifetimeScope("shell");
            //return intermediateScope.BeginLifetimeScope();// 这里不加"shell" 会出错
        }
    }

    public interface IWorker
    {
        void DoWork();
    }

    public class Worker:IWorker
    {
        public void DoWork()
        {
            Console.WriteLine("Hello world");
            Console.Read();
        }
    }

    public abstract class WorkContext
    {
        /// <summary>
        /// Resolves a registered dependency type
        /// </summary>
        /// <typeparam name="T">The type of the dependency</typeparam>
        /// <returns>An instance of the dependency if it could be resolved</returns>
        public abstract T Resolve<T>();
    }

    class WorkContextImplementation : WorkContext
    {
        readonly IComponentContext _componentContext;
        //readonly IWorker _worker;
        public WorkContextImplementation(IComponentContext componentContext)
        {
            _componentContext = componentContext;
            //_worker = componentContext.Resolve<IWorker>();
        }

        public override T Resolve<T>()
        {
            return _componentContext.Resolve<T>();

            //No scope with a Tag matching 'shell' is visible from the scope in which the instance was requested. This generally indicates that a component registered as per-HTTP request is being reqested by a SingleInstance() component (or a similar scenario.) Under the web integration always request dependencies from the DependencyResolver.Current or ILifetimeScopeProvider.RequestLifetime, never from the container itself.
        }
    }
}
