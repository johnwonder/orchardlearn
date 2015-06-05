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
                .ContentPartRecord()        //��ƷId,����ֶ���ʵ�Ǻ�ϵͳ���ĵ�Content�������
                .Column<double>("Price")    //��Ʒ�۸�
                .Column<string>("Brand", column => column.WithLength(200)) //��ƷƷ��
                );

            //����ProductPart
            ContentDefinitionManager.AlterPartDefinition(typeof(ProductPart).Name, cfg => cfg.Attachable());

            //����ProductType
            ContentDefinitionManager.AlterTypeDefinition("Product",
                cfg => cfg.WithPart("ProductPart")  //������Ʒ����
                .WithPart("RoutePart") //·�ɲ���
                .WithPart("BodyPart")                //�ı�����
                );
            return 1;
        }
    }
}