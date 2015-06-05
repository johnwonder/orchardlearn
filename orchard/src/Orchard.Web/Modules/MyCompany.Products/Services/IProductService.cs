using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyCompany.Products.Models;
using Orchard.ContentManagement;
using Orchard;

namespace MyCompany.Products.Services
{
    public interface IProductService : IDependency
    {
        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="versionOptions"></param>
        /// <returns></returns>
        IEnumerable<ProductPart> Get(VersionOptions versionOptions);
    }
}
