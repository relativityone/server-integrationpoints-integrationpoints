using System.Net;
using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class ReadinessController : Controller
    {
        [HttpGet]
        [ActionName("Check")]
        public ActionResult ReadinessCheck()
        {
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
