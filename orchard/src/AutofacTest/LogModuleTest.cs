using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Module = Autofac.Module;
using System.Collections.Concurrent;
using Orchard.Logging;
using Autofac;
using Autofac.Core;
using System.Reflection;
using Orchard.Environment;

namespace AutofacTest
{
    public class  LoggingModule:Module
    {
        private readonly ConcurrentDictionary<string, ILogger>
            _loggerCache;

        public LoggingModule()
        {
            _loggerCache = new ConcurrentDictionary<string, ILogger>();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CastleLoggerFactory>().As<ILoggerFactory>().InstancePerLifetimeScope();
            builder.RegisterType<OrchardLog4netFactory>().As<Castle.Core.Logging.ILoggerFactory>().InstancePerLifetimeScope();

            builder.Register(CreateLogger).As<ILogger>().InstancePerDependency();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            var implementationType = registration.Activator.LimitType;

            var injectors = BuildLoggerInjectors(implementationType).ToArray();
            if (!injectors.Any())
                return;

            registration.Activated += (s, e) => {
                foreach (var injector in injectors)
                {
                    
                    injector(e.Context, e.Instance);
                }
            
            };
         }

        /// <summary>
        /// 返回委托集合
        /// </summary>
        /// <param name="componentType"></param>
        /// <returns></returns>
        private IEnumerable<Action<IComponentContext, object>> BuildLoggerInjectors(Type componentType) {

            var loggerProperties = componentType.GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new
            {

                PropertyInfo = p,
                p.PropertyType,
                IndexParameters = p.GetIndexParameters(),
                Accessors = p.GetAccessors(false)//只返回公共方法

            })
            .Where(x => x.PropertyType == typeof(ILogger))
            .Where(x => x.IndexParameters.Count() == 0)
            .Where(x => x.Accessors.Length != 1 || x.Accessors[0].ReturnType == typeof(void)); //必须有get/set,或者 只有set


            foreach (var entry in loggerProperties)
            {
                var propertyInfo = entry.PropertyInfo;

                yield return (ctx, instance) =>
                {

                    string component = componentType.ToString();
                    var logger = _loggerCache.GetOrAdd(component, key =>
                        ctx.Resolve<ILogger>(new TypedParameter(typeof(Type), componentType)));
                    propertyInfo.SetValue(instance, logger, null);
                };
            }
        }


        private static ILogger CreateLogger(IComponentContext context,
            IEnumerable<Parameter> parameters)
        {
            var loggerFactory = context.Resolve<ILoggerFactory>();
            var containingFactory = parameters.TypedAs<Type>();
            return loggerFactory.CreateLogger(containingFactory);
        }
    }

    class Program
    {
        public static void Main2(string[] args)
        {
            bool isWhiteSpace = Char.IsWhiteSpace('\t');

            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.RegisterType<Thing>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();

            var container = builder.Build();
            var thing = container.Resolve<Thing>();

            Console.WriteLine(thing.Logger.ToString());

            bool isValidpath = "baidu.com".IsValidUrlSegment();

            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            dependencies.Add("~/Modules");
            dependencies.UnionWith(new string[] { "~/Themes","~/Modules" });

            Console.ReadLine();
        }

    }

    public class Thing
    {
        public ILogger Logger { get; set; }

        public void TestMethod()
        {
            Logger.Log(Orchard.Logging.LogLevel.Error, new Exception("测试异常"),"测试异常使用CastleLogger类型");

        }

    }

    public class StubHostEnvironment:HostEnvironment
    {
        public override void RestartAppDomain()
        {
            
        }
    }

    public static class StringExtensions
    {
        private static readonly char[] validSegmentChars = "/?#[]@\"^{}|`<>\t\r\n\f ".ToCharArray();
        public static bool IsValidUrlSegment(this string segment)
        {
            // valid isegment from rfc3987 - http://tools.ietf.org/html/rfc3987#page-8
            // the relevant bits:
            // isegment    = *ipchar
            // ipchar      = iunreserved / pct-encoded / sub-delims / ":" / "@"
            // iunreserved = ALPHA / DIGIT / "-" / "." / "_" / "~" / ucschar
            // pct-encoded = "%" HEXDIG HEXDIG
            // sub-delims  = "!" / "$" / "&" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
            // ucschar     = %xA0-D7FF / %xF900-FDCF / %xFDF0-FFEF / %x10000-1FFFD / %x20000-2FFFD / %x30000-3FFFD / %x40000-4FFFD / %x50000-5FFFD / %x60000-6FFFD / %x70000-7FFFD / %x80000-8FFFD / %x90000-9FFFD / %xA0000-AFFFD / %xB0000-BFFFD / %xC0000-CFFFD / %xD0000-DFFFD / %xE1000-EFFFD
            // 
            // rough blacklist regex == m/^[^/?#[]@"^{}|\s`<>]+$/ (leaving off % to keep the regex simple)

            return !segment.Any(validSegmentChars);
        }

        public static bool Any(this string subject, params char[] chars)
        {
            if (string.IsNullOrEmpty(subject) || chars == null || chars.Length == 0)
            {
                return false;
            }

            Array.Sort(chars);

            for (var i = 0; i < subject.Length; i++)
            {
                char current = subject[i];
                if (Array.BinarySearch(chars, current) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
