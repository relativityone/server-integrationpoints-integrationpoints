using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : BaseController
	{

		public ActionResult Edit(int? objectID)
		{
			return View();
		}


		public ActionResult StepDetails()
		{
			return PartialView("_IntegrationDetailsPartial");
		}

		public ActionResult StepDetails2()
		{
			return PartialView("_IntegrationDetailsPartial2");
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
