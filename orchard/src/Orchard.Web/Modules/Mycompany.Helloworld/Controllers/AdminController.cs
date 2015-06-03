using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using Orchard.Themes;
using Mycompany.Helloworld.Services;
using Orchard.Core.Contents.Controllers;
using MyCompany.Helloworld.ViewModels;

namespace MyCompany.Helloworld.Controllers {
    
    /// <summary>
    /// ���һ��Themed�����ԣ����������ֵ����ݲ�����Orchard��Ƥ������ʾ
    /// </summary>
    [Themed]
    public class AdminController : Controller {
        private readonly ITextService _textService;

        public AdminController(ITextService textService)
        {
            _textService = textService;
        }

        public ActionResult Index()
        {
            var viewModel = new EditTextViewModel();
            var textRecord = _textService.GetText();

            if (textRecord != null)
            {
                viewModel.Content = textRecord.Content;
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Save")]
        public ActionResult IndexPost()
        {
            var viewModel = new EditTextViewModel();

            if (!TryUpdateModel(viewModel))
            {
                return Index();
            }

            _textService.UpdateText(viewModel.Content);

            return RedirectToAction("Index");
        }
    }
}
