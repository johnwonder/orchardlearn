using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using MyCompany.Products.Models;
using Orchard.Data;

namespace MyCompany.Products.Handlers
{
    public class ProductHandler:ContentHandler
    {
        /// <summary>
        /// http://www.cnblogs.com/esshs/archive/2011/06/13/2079937.html
        /// 处理器相当于内容部件的Filter
        /// 目前仅针对该产品内容部件的存储进行定义
        /// </summary>
        /// <param name="repository"></param>
        public ProductHandler(IRepository<ProductRecord> repository)
        {
            //定义ProductPart的数据存储是通过IRepository<ProductRecord>进行处理的
            //IRepository就把它理解为数据访问层
            Filters.Add(StorageFilter.For(repository));
        }
    }
}