using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : Controller
	{
		public ActionResult Edit()
		{
			return View();
		}
	}
}
