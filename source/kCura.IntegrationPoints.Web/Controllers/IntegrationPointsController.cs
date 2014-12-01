using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : BaseController
	{
		private IntegrationPointReader _reader;
		public IntegrationPointsController(IntegrationPointReader reader)
		{
			_reader = reader;
		}
		public ActionResult Edit(int? objectID)
		{
			return View();
		}
		
		public ActionResult StepDetails()
		{
			return PartialView("_IntegrationDetailsPartial");
		}


		public ActionResult Details(int id)
		{
			var integrationViewModel = _reader.ReadIntegrationPoint(id);
			return View(integrationViewModel);
		}

	}
}
