using System;
using System.Collections.Generic;
using Autofac;
using log4net.Appender;
using log4net.Core;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Logging;
using Orchard.Tests.Environment;
using ILogger = Orchard.Logging.ILogger;
using ILoggerFactory = Orchard.Logging.ILoggerFactory;
using NullLogger = Orchard.Logging.NullLogger;

namespace Orchard.Tests.Logging {
    [TestFixture]
    public class LoggingModuleTests {
        [Test]
        public void LoggingModuleWillSetLoggerProperty() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.RegisterType<Thing>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();
            var container = builder.Build();
            var thing = container.Resolve<Thing>();
            Assert.That(thing.Logger, Is.Not.Null);
        }

        [Test]
        public void LoggerFactoryIsPassedTheTypeOfTheContainingInstance() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.RegisterType<Thing>();
            var stubFactory = new StubFactory();
            builder.RegisterInstance(stubFactory).As<ILoggerFactory>();

            var container = builder.Build();
            var thing = container.Resolve<Thing>();
            Assert.That(thing.Logger, Is.Not.Null);
            Assert.That(stubFactory.CalledType, Is.EqualTo(typeof(Thing)));
        }

        [Test]
        public void CreateTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.RegisterType<Thing>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();


            var container = builder.Build();
            var thing = container.Resolve<Thing>();
            thing.TestMethod();
        }

        public class StubFactory : ILoggerFactory {
            public ILogger CreateLogger(Type type) {
                CalledType = type;
                return NullLogger.Instance;
            }

            public Type CalledType { get; set; }
        }

        [Test]
        public void DefaultLoggerConfigurationUsesCastleLoggerFactoryOverTraceSource() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.RegisterType<Thing>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();
            var container = builder.Build();

            log4net.Config.BasicConfigurator.Configure(new MemoryAppender());

            var thing = container.Resolve<Thing>();
            Assert.That(thing.Logger, Is.Not.Null);

            MemoryAppender.Messages.Clear();
            thing.Logger.Error("-boom{0}-", 42);
            Assert.That(MemoryAppender.Messages, Has.Some.StringContaining("-boom42-"));

            MemoryAppender.Messages.Clear();
            thing.Logger.Warning(new ApplicationException("problem"), "crash");
            Assert.That(MemoryAppender.Messages, Has.Some.StringContaining("problem"));
            Assert.That(MemoryAppender.Messages, Has.Some.StringContaining("crash"));
            Assert.That(MemoryAppender.Messages, Has.Some.StringContaining("ApplicationException"));
        }
    }

    public class Thing {
        public ILogger Logger { get; set; }

        public Thing()
        {
            Logger = NullLogger.Instance;
        }

        public void TestMethod()
       {
         //这使用Log方法则 Logger属性 会被设置为 CastleLogger类；使用Error方法 Logger属性 会被设置为OrchardLog4netLogger类
           //Logger.IsEnabled(LogLevel.Information);
            Logger.Log(Orchard.Logging.LogLevel.Error,new Exception("测试异常"),"测试异常使用CastleLogger类型");
           // Logger.IsEnabled(LogLevel.Error);
            Logger.Error(new Exception("测试异常"),"测试异常使用OrchardLog4netLogger类型");
       }
    }

    public class MemoryAppender : IAppender {
        static MemoryAppender() {
            Messages = new List<string>();
        }

        public static List<string> Messages { get; set; }

        public void DoAppend(LoggingEvent loggingEvent) {
            if (loggingEvent.ExceptionObject != null) {
                lock (Messages) Messages.Add(string.Format("{0} {1} {2}",
                    loggingEvent.ExceptionObject.GetType().Name,
                    loggingEvent.ExceptionObject.Message,
                    loggingEvent.RenderedMessage));
            } else lock (Messages) Messages.Add(loggingEvent.RenderedMessage); 
        }

        public void Close() { }
        public string Name { get; set; }
    }
}
