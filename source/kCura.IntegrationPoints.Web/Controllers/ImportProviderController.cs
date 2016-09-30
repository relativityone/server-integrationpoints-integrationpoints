using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class ImportProviderController : BaseController
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
