using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Automapping;
using NHibernate.Tool.hbm2ddl;

namespace NHibernateTest
{
    class AutoMapTest
    {
        public static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008
                .ConnectionString(s => s.Server(".")
                .Database("MyNHibernate")
                .TrustedConnection()))
                //.Mappings(m => m.FluentMappings.AddFromAssembly(typeof(Program).Assembly).ExportTo("c:\\test"))
            .Mappings(m => m.AutoMappings.Add(CreateAutomappings()))
            .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, true))
            .BuildSessionFactory();

        }


        private static AutoPersistenceModel CreateAutomappings()
        {
            //AutoMap.Assembly(Assembly.Load("ClassLibrary1"), new StoreConfiguration())  
            //添加需要映射的程序集，在添加时可以使用默认的DefaultAutomappingConfiguration也可以继承该类重写ShouldMap方法来指定映射的命名空间  
            return AutoMap
                .Assembly(typeof(Program).Assembly, new StoreConfiguration())
                //Conventions.Setup()方法是将映射的具体规则绑定到AutoMapping配置中，如下配置了：  
                //DefualtTableNameConvertion默认表命名方式  
                //DefaultPrimarykeyConvention默认主键命名方式  
                //DefualtStringLengthConvertion默认的字符串长度  
                //DefaultReferenceConvention默认的外键创建方法  
                //DefaultHasManyConvention默认的一对多的创建方法  
                //DefualtHasOneConvertion默认的一对一的创建方法  
                //HasManyToManyConvention默认的多对多的创建方法  
                .Conventions.Setup(con =>
                {
                    //con.Add<DefualtTableNameConvertion>();
                    //con.Add<DefaultPrimarykeyConvention>();
                    //con.Add<DefualtStringLengthConvertion>();
                    //con.Add<DefaultReferenceConvention>();
                    //con.Add<DefaultHasManyConvention>();
                    //con.Add<DefualtHasOneConvertion>();
                    //con.Add<HasManyToManyConvention>();
                });
        }

        private class StoreConfiguration : DefaultAutomappingConfiguration
        {
            public override bool ShouldMap(Type type)
            {
                return type.Namespace == "NHibernateTest.Entity";
            }
        }  

    }
}
