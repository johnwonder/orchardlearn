using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Mvc.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Mycompany.Helloworld
{
    /// <summary>
    /// 定义模块所用到的路由，在Orchard中定义路由需实现IRouteProvider接口
    /// </summary>
    public class Routes:IRouteProvider
    {

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[]{
             
                    new RouteDescriptor{
                    
                          Priority = 5,
                          Route = new Route(
                              
                                "Helloworld",
                                new RouteValueDictionary{
                                
                                        {"area","MyCompany.HelloWorld"},
                                        {"controller","Home"},
                                        {"action","Index"}
                                },
                                new RouteValueDictionary(),
                                new RouteValueDictionary { { "area","MyCompany.HelloWorld" } },
                                new MvcRouteHandler()
                              
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
    }
}