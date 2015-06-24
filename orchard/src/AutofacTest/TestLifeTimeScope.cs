using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using System.Collections;

namespace AutofacTest
{
    public class TestLifeTimeScope
    {
        public static void EnumerablesFromDifferentLifetimeScope()
        {
            var rootBuilder = new ContainerBuilder();
            rootBuilder.RegisterType<Foo1>().As<IFoo>();
            var rootContainer = rootBuilder.Build();

            var scopeA = rootContainer.BeginLifetimeScope(
                    scopeBuilder => scopeBuilder.RegisterType<Foo2>().As<IFoo>()
                );
            var arrayA = scopeA.Resolve<IEnumerable<IFoo>>().ToArray();

            Console.WriteLine(arrayA.Count());

            IEnumerator enumerator = arrayA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object foo = enumerator.Current;
                if (foo != null)
                    Console.WriteLine(foo.GetType().FullName);
            }
        }
    }

    public interface IFoo
    {

    }

    public class Foo1:IFoo
    {

    }

    public class Foo2:IFoo
    {
        
    }

    public class Foo3:IFoo
    {

    }
}
