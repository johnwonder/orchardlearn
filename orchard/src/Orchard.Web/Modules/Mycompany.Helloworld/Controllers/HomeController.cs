using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using Orchard.Themes;
using Mycompany.Helloworld.Services;

namespace MyCompany.Helloworld.Controllers {
    
    /// <summary>
    /// ���һ��Themed�����ԣ����������ֵ����ݲ�����Orchard��Ƥ������ʾ
    /// </summary>
    [Themed]
    public class HomeController : Controller {

        private readonly ITextService _textService;

        public HomeController(ITextService textService)
        {
            _textService = textService;
          
        }

        public ActionResult Index()
        {
            string model = "Hello World";

            var textRecord = _textService.GetText();
            if (textRecord != null)
            {
                model = textRecord.Content;
            }

            return View((object)model);
        }

     
    }
}
