using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Autofac;

namespace ConsoleSocketDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            //builder.RegisterDecorator<
        }
    }

    //InstancePerDependency
    //对每一个依赖或每一次调用创建一个新的唯一的实例。这也是默认的创建实例的方式。

    //InstancePerLifetimeScope
    //在每一个生命周期中，每一个依赖或调用创建一个单一的共享的示例，且每一个不同的生命周期域，实例是唯一的，不共享的。

    //InstancePerMatchingLifetimeScope
    //在一个做标识的生命周期域中，每一个依赖或调用创建一个单一的共享的实例。打了标识的生命周期域中的子标识域中可以共享父域中的实例。若在整个继承层次中没有找到打标识的生命周期域，会抛出异常。

    //InstancePerOwned
    //在一个生命周期域中所拥有的实例创建的生命周期中，每一个依赖组件或调用Resolve()方法创建一个单一的共享的实例，并且子生命周期共享父生命周期域中的实例。

    //public class MyDataDeal:DataDeal
    //{
    //    public event LoginEventHandler Login;
    //    public event LoginBackEventHandler LoginBack;
    //    public event TxtMessageReceivedEventHandler TxtMessageReceived;
    //}


    interface ICommndHandler { 
    
    }

    class CommandHandler:ICommndHandler
    {

    }


    class TransactedCommandHandlerL:ICommndHandler
    {
        //public TransactedCommandHandlerL(ICommndHandler decorated,ISession session)
        //{

        //}
    }
}
