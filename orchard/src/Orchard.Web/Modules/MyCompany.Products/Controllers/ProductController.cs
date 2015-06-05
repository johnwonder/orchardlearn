using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using MyCompany.Products.Services;
using Orchard.Localization;
using Orchard;
using Orchard.Themes;

namespace MyCompany.Products.Controllers
{
    [Themed]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ProductController(IOrchardServices services, IProductService productService)
        {
            _productService = productService;
            Services = services;
        }

        public ActionResult List()
        {
            var list = Services.New.List();
            list.AddRange(_productService.Get(VersionOptions.Latest).Select(p =>
            {
                var product = Services.ContentManager.BuildDisplay(p, "Summary");
                return product;
            }));

            dynamic viewModel = Services.New.ViewModel()
                .ContentItems(list);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }
    }
}
