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
		public IntegrationPointsController()
		{
			
		}

		public ActionResult Edit(int? id)
		{
			return View();
		}

		public ActionResult Index()
		{
			return View(); 
		}
	}
}
