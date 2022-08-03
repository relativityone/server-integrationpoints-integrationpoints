using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class ModalsController : Controller
    {
        public ActionResult ExportRenamedFieldsView()
        {
            return PartialView();
        }

        public ActionResult ExportFileNamingOptionView()
        {
            return PartialView();
        }

        public ActionResult CreateProductionSetModalView()
        {
            return PartialView();
        }

        public ActionResult CreatingProductionSetModalView()
        {
            return PartialView();
        }
    }
}
