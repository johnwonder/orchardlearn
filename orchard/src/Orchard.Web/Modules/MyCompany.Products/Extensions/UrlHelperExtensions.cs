using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyCompany.Products.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string ProductsForAdmin(this UrlHelper urlHelper)
        {
            return urlHelper.Action("Index", "Admin", new { area = "MyCompany.Products" });
        }

        /// <summary>
        /// 在Index页面用Url.ProductCreate
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <returns></returns>
        public static string ProductCreate(this UrlHelper urlHelper)
        {
            return urlHelper.Action("Create", "Admin", new { area = "MyCompany.Products" });
        }
    }
}