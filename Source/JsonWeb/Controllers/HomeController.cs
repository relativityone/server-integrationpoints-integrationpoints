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
	}
}
