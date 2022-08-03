using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class TooltipController : Controller
    {
        public ActionResult TooltipView()
        {
            return PartialView();
        }
    }
}
