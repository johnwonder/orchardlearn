using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using MyCompany.Products.Models;

namespace MyCompany.Products {
    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("ProductRecord", table => table
                .ContentPartRecord()        //产品Id,这个字段其实是和系统核心的Content表关联的
                .Column<double>("Price")    //产品价格
                .Column<string>("Brand", column => column.WithLength(200)) //产品品牌
                );

            //定义ProductPart
            ContentDefinitionManager.AlterPartDefinition(typeof(ProductPart).Name, cfg => cfg.Attachable());

            //定义ProductType
            ContentDefinitionManager.AlterTypeDefinition("Product",
                cfg => cfg.WithPart("ProductPart")  //包含产品部件
                .WithPart("RoutePart") //路由部件
                .WithPart("BodyPart")                //文本部件
                );
            return 1;
        }
    }
}