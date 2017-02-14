using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class ModalsController : BaseController
	{
		public ActionResult AuthenticationModalView()
		{
			return PartialView();
		}
	}
}
