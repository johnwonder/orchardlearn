using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyCompany.Products.Models;
using Orchard.ContentManagement;
//using Orchard.Core.Routable.Models;
using JetBrains.Annotations;

namespace MyCompany.Products.Services
{
    [UsedImplicitly]
    public class ProductService : IProductService
    {
        private readonly IContentManager _contentManager;

        public ProductService(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        #region IProductService 成员

        public IEnumerable<ProductPart> Get(VersionOptions versionOptions)
        {
            //从数据库获取ProductPart
            return _contentManager.Query<ProductPart, ProductRecord>(versionOptions)
                //.Join<RoutePartRecord>()
                //.OrderBy(br => br.Title)
                .List();
        }

        #endregion
    }
}