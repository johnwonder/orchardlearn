using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Features.Metadata;

namespace AutofacTest
{
    /// <summary>
    /// Autofac测试
    /// http://www.cnblogs.com/wolegequ/archive/2012/06/09/2543320.html
    /// </summary>
    public  class  AutofacMeta
    {
        //static void Main(string[] args)
        //{
        //    //TestAutofacMeta();
        //    TestAutofacDelegate();
        //    Console.ReadLine();
        //}

        public   static void  TestAutofacMeta()
        {
            var builder = new  ContainerBuilder();
 
 
           builder.RegisterType<SqlServerDataServicesProvider>().As<IDataServicesProvider>();

            var container = builder.Build();

            IEnumerable<Meta<CreateDataServicesProvider>> _Providers = container.Resolve<IEnumerable<Meta<CreateDataServicesProvider>>>();

            foreach (var item in _Providers)
            {
                item.Value("ss", "11").BuildConfiguration();
            }
        }

        static void TestAutofacDelegate()
        {
            var builder = new ContainerBuilder();


            builder.RegisterType<SqlServerDataServicesProvider>().As<IDataServicesProvider>();

            var container = builder.Build();

            IEnumerable<CreateDataServicesProvider> _Providers = container.Resolve<IEnumerable<CreateDataServicesProvider>>();

            foreach (var item in _Providers)
            {
                item("ss", "11").BuildConfiguration();
            }
        }
    }

   public interface  IDataServicesProvider{
        
         void BuildConfiguration();
   }

   public delegate IDataServicesProvider CreateDataServicesProvider(string dataFolder, string connectionString);

   public class SqlServerDataServicesProvider : IDataServicesProvider
   {
       private readonly string _dataFolder;
       private readonly string _connectionString;

       public SqlServerDataServicesProvider(string dataFolder, string connectionString)
       {
           _dataFolder = dataFolder;
           _connectionString = connectionString;
       }

       public void BuildConfiguration()
       {
           Console.WriteLine(_dataFolder);
           Console.WriteLine(_connectionString);
       }
   }
}
