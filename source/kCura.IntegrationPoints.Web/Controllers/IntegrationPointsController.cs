using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Core.Models;
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


		public ActionResult Details(int? objectID)
		{
			var integrationViewModel = new IntegrationViewModel()
			{
				Name = "Main Active Directory Sync",Overwrite = "Append", SourceProvider = "LDAP",Destination = "Custodian", EnableScheduler = true, Frequency = "Daily",StartDate = new DateTime(2014,7,13),
				EndDate = null,
				ScheduleTime = new DateTime(2014, 12, 12, 5, 0, 0),
				ConnectionPath = "kcura.corp",
				FilterString = "&(objectCategory=person)(objectClass=user)",
				Authentication = "fastBind",
				Username = "kcura\rellockdown",
				Password = "********",
				NestedItems = "*******",
				NextRun = new DateTime(2014, 2, 7, 5, 4, 0),
				LastRun = new DateTime(2014, 2, 7, 5, 4, 0)
			};
			return View(integrationViewModel);
		}

	}
}
