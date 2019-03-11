using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class ImportProviderController : Controller
    {
        public ActionResult ImportSettings()
        {
            return View();
        }

        public ActionResult ImportPreview()
        {
            return View();
        }
    }
}
