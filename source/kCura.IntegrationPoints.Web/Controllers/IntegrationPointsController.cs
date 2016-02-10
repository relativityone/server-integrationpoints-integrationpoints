using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Toggles;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : BaseController
	{
		private readonly IntegrationPointService _reader;
		private readonly RSAPIRdoQuery _rdoQuery;
		private readonly ITabService _tabService;
		private readonly IPermissionService _permissionService;

		public IntegrationPointsController(
			IntegrationPointService reader, 
			RSAPIRdoQuery relativityRdoQuery, 
			ITabService tabService, 
			IPermissionService permissionService)
		{
			_reader = reader;
			_rdoQuery = relativityRdoQuery;
			_tabService = tabService;
			_permissionService = permissionService;
		}

		public ActionResult Edit(int? id)
		{
			var objectTypeId = _rdoQuery.GetObjectTypeID(Data.ObjectTypes.IntegrationPoint);
			var tabID = _tabService.GetTabId(objectTypeId);
			var objectID = _rdoQuery.GetObjectType(objectTypeId).ParentArtifact.ArtifactID;
			var previousURL = "List.aspx?AppID=" + SessionService.WorkspaceID + "&ArtifactID=" + objectID + "&ArtifactTypeID=" + objectTypeId + "&SelectedTab=" + tabID;
			if (_permissionService.userCanImport(SessionService.WorkspaceUserID))
			{
				return View(new EditPoint
				{
					AppID = SessionService.WorkspaceID,
					ArtifactID = id.GetValueOrDefault(0),
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

		public ActionResult StepDetails3()
		{
			return PartialView("_IntegrationMapFields");
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
