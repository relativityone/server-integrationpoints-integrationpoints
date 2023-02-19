using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class FileshareController : Controller
    {
        public ActionResult SavedSearchPicker()
        {
            return PartialView();
        }

        public ActionResult ListPicker()
        {
            return PartialView();
        }
    }
}
