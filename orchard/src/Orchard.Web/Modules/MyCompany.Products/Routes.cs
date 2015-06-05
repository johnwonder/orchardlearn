using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Mvc.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace MyCompany.Products
{
    /// <summary>
    /// 定义模块所用到的路由，在Orchard中定义路由需要实现IRouteProvider接口
    /// </summary>
    public class Routes : IRouteProvider
    {
        #region IRouteProvider 成员

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] 
            {
                new RouteDescriptor 
                {                    
                    Priority = 5,                           //优先级（作用暂不清楚，留着以后研究）
                    Route = new Route
                    (
                        "Products",                       //路由的 URL 模式。
                        new RouteValueDictionary            //要在 URL 不包含所有参数时使用的值。默认执行HomeController中Index action。
                        {
                            {"area", "MyCompany.Products"},
                            {"controller", "Product"},
                            {"action", "List"}
                        },
                        new RouteValueDictionary(),         //一个用于指定 URL 参数的有效值的正则表达式。
                        new RouteValueDictionary {{"area", "MyCompany.Products"}},    //传递到路由处理程序但未用于确定该路由是否匹配特定 URL 模式的自定义值。这些值会传递到路由处理程序，以便用于处理请求。
                        new MvcRouteHandler()       //处理路由请求的对象。
                    )
                }
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        #endregion
    }
}