using System;
using System.Web.Mvc;

namespace JsonWeb.Controllers
{
	public class HomeController : Controller
	{
		//
		// GET: /Home/

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult GetJson()
		{
			throw new NotImplementedException("Get json not implemented");
		}
	}
}
