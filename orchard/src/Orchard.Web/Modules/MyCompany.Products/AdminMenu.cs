using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.UI.Navigation;
using Orchard.Security;
using Orchard.Localization;

namespace MyCompany.Products
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }

        #region INavigationProvider 成员

        /// <summary>
        /// 指定这个菜单是一个管理菜单
        /// </summary>
        public string MenuName
        {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("products")       //设置菜单图标（Orchard中默认约定调用Styles目录下的menu.xxxxxx-admin.css样式文件来显示菜单前面的图标）
                .Add(
                    T("Products"),               //菜单文本
                    "6",                         //菜单位置（Orchard会根据这个值对菜单进行排序）
                    menu => menu.Action("Index", "Admin", new { area = "MyCompany.Products" })    //定义菜单所执行的路由
                 );
        }

        #endregion
    }
}
