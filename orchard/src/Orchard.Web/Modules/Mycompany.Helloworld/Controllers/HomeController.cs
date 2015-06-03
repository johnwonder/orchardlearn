using System.Web.Mvc;
using Orchard.Localization;
using Orchard;
using Orchard.Themes;

namespace MyCompany.Helloworld.Controllers {
    
    /// <summary>
    /// ���һ��Themed�����ԣ����������ֵ����ݲ�����Orchard��Ƥ������ʾ
    /// </summary>
    [Themed]
    public class HomeController : Controller {
        public IOrchardServices Services { get; set; }

        public HomeController(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index()
        {
            string model = "Hello World";
            return View((object)model);
        }

        public Localizer T { get; set; }
    }
}
