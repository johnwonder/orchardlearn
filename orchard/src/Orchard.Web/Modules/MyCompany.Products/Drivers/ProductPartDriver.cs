using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyCompany.Products.Models;
using Orchard.ContentManagement.Drivers;

namespace MyCompany.Products.Drivers
{
    /// <summary>
    /// Driver相当于内容部件的Controller
    /// </summary>
    public class ProductPartDriver:ContentPartDriver<ProductPart>
    {
        /// <summary>
        /// 界面显示时执行(相当于Action)
        /// </summary>
        /// <param name="part">相当于此Part的Model</param>
        /// <param name="displayType">显示类型 (如："details" (详情显示) 或summary(摘要显示) </param>
        /// <param name="shapeHelper">类似视图引擎之类的东西，可以根据相应显示的动态对象去找对应的显示模板(相当于View)</param>
        /// <returns>相当于返回一个ActionResult, Orchard框架会针对这个返回值进行相应处理</returns>
        protected override DriverResult Display(ProductPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Product", () => shapeHelper.Parts_Product(
                    Price : part.Price,
                    Brand: part.Brand
                ));
        }
        /// <summary>
        /// 编辑界面显示时执行
        /// </summary>
        /// <param name="part"></param>
        /// <param name="shapeHelper"></param>
        /// <returns></returns>
        protected override DriverResult Editor(ProductPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Product_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Product",
                    Model: part,
                    Prefix: Prefix
                    ));
        }
        /// <summary>
        /// 编辑界面提交时执行(Post)
        /// </summary>
        /// <param name="part"></param>
        /// <param name="updater"></param>
        /// <param name="shapeHelper"></param>
        /// <returns></returns>
        protected override DriverResult Editor(ProductPart part, Orchard.ContentManagement.IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}