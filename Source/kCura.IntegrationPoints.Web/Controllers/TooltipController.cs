using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class TooltipController : BaseController
	{
		public ActionResult TooltipView()
		{
			return PartialView();
		}
	}
}
