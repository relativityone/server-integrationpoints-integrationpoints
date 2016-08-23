using System.IO;
using System.Net;
using System.Web.Mvc;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Web.Models;
using Relativity.API;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : BaseController
	{
		private readonly IIntegrationPointService _reader;
		private readonly RSAPIRdoQuery _rdoQuery;
		private readonly ITabService _tabService;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAPILog _apiLog;

		public IntegrationPointsController(
			IIntegrationPointService reader,
			RSAPIRdoQuery relativityRdoQuery,
			ITabService tabService,
			IRepositoryFactory repositoryFactory)
		{
			_reader = reader;
			_rdoQuery = relativityRdoQuery;
			_tabService = tabService;
			_repositoryFactory = repositoryFactory;
			_apiLog = ConnectionHelper.Helper().GetLoggerFactory().GetLogger().ForContext<IntegrationPointsController>();
		}

		public ActionResult Edit(int? artifactId)
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(SessionService.WorkspaceID);

			var objectTypeId = _rdoQuery.GetObjectTypeID(Data.ObjectTypes.IntegrationPoint);
			var tabID = _tabService.GetTabId(objectTypeId);
			var objectID = _rdoQuery.GetObjectType(objectTypeId).ParentArtifact.ArtifactID;
			var previousURL = "List.aspx?AppID=" + SessionService.WorkspaceID + "&ArtifactID=" + objectID + "&ArtifactTypeID=" + objectTypeId + "&SelectedTab=" + tabID;
			if (permissionRepository.UserCanImport())
			{
				return View(new EditPoint
				{
					AppID = SessionService.WorkspaceID,
					ArtifactID = artifactId.GetValueOrDefault(0),
					UserID = base.SessionService.UserID,
					CaseUserID = base.SessionService.WorkspaceUserID,
					URL = previousURL,
				});
			}
			return View("NotEnoughPermission", new EditPoint { URL = previousURL });
		}

		public ActionResult StepDetails()
		{
			return PartialView("_IntegrationDetailsPartial");
		}

        public ActionResult StepDetails3Reversed()
        {
            return PartialView("_IntegrationMapFieldsReversed");
        }
        public ActionResult StepDetails3()
		{
			return PartialView("_IntegrationMapFields");
		}

        public ActionResult StepDetails3Export()
        {
            return PartialView("ExportProviderFields");
        }

		public ActionResult ConfigurationDetail()
		{
			return PartialView("_Configuration");
		}

		public ActionResult LDAPConfiguration()
		{
			return View("LDAPConfiguration", "_StepLayout");
		}

		public ActionResult RelativityProviderConfiguration()
		{
			return View("RelativityProviderConfiguration", "_StepLayout");
		}

		public ActionResult ExportProviderConfiguration()
		{
			return View("ExportProviderConfiguration", "_StepLayout");
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