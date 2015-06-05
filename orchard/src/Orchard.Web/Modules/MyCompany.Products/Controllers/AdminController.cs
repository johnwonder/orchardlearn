using System.Linq;
using System.Web.Mvc;
using MyCompany.Products.Extensions;
using MyCompany.Products.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using MyCompany.Products.Services;

namespace MyCompany.Products.Controllers
{
    //需要将输入校验设为false否则富文本数据无法保存
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel 
    {
        private readonly IProductService _productService;

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public AdminController(IOrchardServices services, IProductService productService)
        {
            _productService = productService;
            Services = services;
        }

        public ActionResult Index()
        {
            var list = Services.New.List();
            list.AddRange(_productService.Get(VersionOptions.Latest).Select(p =>
            {
                var product = Services.ContentManager.BuildDisplay(p, "SummaryAdmin");//找到Orchard.Core/ Contents/ Views/ Content.SummaryAdmin.cshtml
                return product;
            }));

            dynamic viewModel = Services.New.ViewModel()
                .ContentItems(list);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }

        /// <summary>
        /// 本示例只是实现了创建画面，编辑画面还是利用了Orchard本身内容管理的功能。
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ProductPart product = Services.ContentManager.New<ProductPart>("Product");
            if (product == null)
                return HttpNotFound();

            dynamic model = Services.ContentManager.BuildEditor(product);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost()
        {
            var product = Services.ContentManager.New<ProductPart>("Product");

            Services.ContentManager.Create(product, VersionOptions.Draft);
            dynamic model = Services.ContentManager.UpdateEditor(product, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.ContentManager.Publish(product.ContentItem);
            //_blogPathConstraint.AddPath(blog.As<IRoutableAspect>().Path);

            return Redirect(Url.ProductsForAdmin());
        }



        #region IUpdateModel 成员

        //显示实现IUpdateModel接口

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        #endregion
    }
}