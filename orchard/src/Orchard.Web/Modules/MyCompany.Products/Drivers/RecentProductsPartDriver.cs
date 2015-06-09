using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyCompany.Products.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace MyCompany.Products.Drivers
{
    public class RecentProductsPartDriver : ContentPartDriver<RecentProductsPart>
    {
        private readonly IContentManager _contentManager;

        public RecentProductsPartDriver(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        protected override DriverResult Display(RecentProductsPart part, string displayType, dynamic shapeHelper)
        {
            var products = _contentManager.Query<ProductPart,ProductRecord>()
                //.OrderByDescending<ProductRecord,int>(p => p.Id,)           //通过产品Id到排序取最新数据
                .Slice(0, part.Count)                                       //取最新多少条数据
                .Select(ci => ci.As<ProductPart>());

            var productsList = shapeHelper.List();          //构建一个显示列表，并且指定已摘要的形式显示显示列表（这部分的内容还有待于学习Orchard视图引擎后理解）
            productsList.AddRange(products.Select(bp => _contentManager.BuildDisplay(bp, "Summary")));
            return ContentShape("Parts_RecentProducts", () => shapeHelper.Parts_RecentProducts(ContentItems: productsList));
        }

        protected override DriverResult Editor(RecentProductsPart part, dynamic shapeHelper)
        {

            return ContentShape("Parts_RecentProducts_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/RecentProducts", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(RecentProductsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }    
    }
}