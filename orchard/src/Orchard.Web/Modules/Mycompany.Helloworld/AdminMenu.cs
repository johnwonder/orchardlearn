using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.UI.Navigation;
using Orchard.Localization;

namespace Mycompany.Helloworld
{
    public class AdminMenu:INavigationProvider
    {
        public Localizer T { get; set; }

        /// <summary>
        /// 指定这个菜单是个管理菜单
        /// </summary>
        public string MenuName { 
            get {
                return "admin";
                    }
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("helloworld")
                .Add(
                    T("Hello World"),
                    "5",
                    menu => menu.Action("Index", "Admin", new  { area = "MyCompany.Helloworld" })
                );
        }
    }
}