using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace MyCompany.Helloworld {
    public class Migrations : DataMigrationImpl {

        public int Create() {


            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateTable("TextRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Content", column => column.WithLength(200)));

            return 2;
        }
    }
}