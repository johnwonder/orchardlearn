using System.Web.Mvc;
using Orchard.Localization;
using Orchard;

namespace MyCompany.Helloworld.Controllers {
    public class HomeController : Controller {
        public IOrchardServices Services { get; set; }

        public HomeController(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}
