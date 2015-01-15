using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Models;
namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : BaseController
	{
		private readonly IntegrationPointService _reader;
		private readonly RdoSynchronizer _rdoSynchronizer;
		public IntegrationPointsController(IntegrationPointService reader, RdoSynchronizer rdosynchronizer)
		{
			_rdoSynchronizer = rdosynchronizer;
			_reader = reader;
		}

		public ActionResult Edit(int? id)
		{
			return View(new EditPoint
			{
				AppID = SessionService.WorkspaceID,
				ArtifactID = id.GetValueOrDefault(0),
				UserID = base.SessionService.UserID
			});
		}

		public ActionResult StepDetails()
		{
			return PartialView("_IntegrationDetailsPartial");
		}

		public ActionResult StepDetails3()
		{
			return PartialView("_IntegrationDetailsPartial3");
		}

		public ActionResult ConfigurationDetail()
		{
			return PartialView("_Configuration");
		}

		public ActionResult LDAPConfiguration()
		{
			return View("LDAPConfiguration", "_StepLayout");
		}

		public ActionResult Details(int id)
		{
			var integrationViewModel = _reader.ReadIntegrationPoint(id);

			var model = new Models.IpDetailModel();
			model.DataModel = integrationViewModel;

			return View(model);
		}

		[HttpPost]
		public ActionResult CheckLdap(LDAPSettings model)
		{
			var service = new LDAPProvider.LDAPService(model);
			service.InitializeConnection();
			bool isAuthenticated = service.IsAuthenticated();
			return isAuthenticated ? JsonNetResult(new object { }) : JsonNetResult(new { }, HttpStatusCode.BadRequest);
		}

	}
}
