using System;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public abstract class IntegrationPointBaseController : BaseController
	{
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ITabService _tabService;

		protected IntegrationPointBaseController(IObjectTypeRepository objectTypeRepository, IRepositoryFactory repositoryFactory, ITabService tabService, ILDAPServiceFactory ldapServiceFactory)
		{
			_objectTypeRepository = objectTypeRepository;
			_repositoryFactory = repositoryFactory;
			_tabService = tabService;
		}

		protected abstract string ObjectTypeGuid { get; }
		protected abstract string ObjectType { get; }
		protected abstract string APIControllerName { get; }

		public ActionResult Edit(int? artifactId)
		{
			var objectTypeId = _objectTypeRepository.GetObjectTypeID(ObjectType);
			var tabID = _tabService.GetTabId(objectTypeId);
			var objectID = _objectTypeRepository.GetObjectType(objectTypeId).ParentArtifactId;
			var previousURL = "List.aspx?AppID=" + SessionService.WorkspaceID + "&ArtifactID=" + objectID + "&ArtifactTypeID=" + objectTypeId + "&SelectedTab=" + tabID;
			if (HasPermissions(artifactId))
			{
				return View("~/Views/IntegrationPoints/Edit.cshtml", new EditPoint
				{
					AppID = SessionService.WorkspaceID,
					ArtifactID = artifactId.GetValueOrDefault(0),
					UserID = SessionService.UserID,
					CaseUserID = SessionService.WorkspaceUserID,
					URL = previousURL,
					APIControllerName = APIControllerName,
					ArtifactTypeName = ObjectType
				});
			}
			return View("~/Views/IntegrationPoints/NotEnoughPermission.cshtml", new EditPoint {URL = previousURL});
		}

		protected bool HasPermissions(int? artifactId)
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(SessionService.WorkspaceID);
			bool canImport = permissionRepository.UserCanImport();
			bool canAddOrEdit = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuid),
				artifactId.HasValue ? ArtifactPermission.Edit : ArtifactPermission.Create);
			bool canEditExistingIp = !artifactId.HasValue ||
									permissionRepository.UserHasArtifactInstancePermission(new Guid(ObjectTypeGuid), artifactId.Value, ArtifactPermission.Edit);
			return canImport && canAddOrEdit && canEditExistingIp;
		}

		public ActionResult Details(int id)
		{
			var integrationViewModel = GetIntegrationPointBaseModel(id);

			var model = new IpDetailModel {DataModel = integrationViewModel};

			return View("~/Views/IntegrationPoints/Details.cshtml", model);
		}

		protected abstract IntegrationPointModelBase GetIntegrationPointBaseModel(int id);

		public ActionResult StepDetails()
		{
			return PartialView("~/Views/IntegrationPoints/_IntegrationDetailsPartial.cshtml");
		}

		public ActionResult StepDetails3()
		{
			return PartialView("~/Views/IntegrationPoints/_IntegrationMapFields.cshtml");
		}

		public ActionResult ExportProviderFields()
		{
			return PartialView("~/Views/IntegrationPoints/ExportProviderFields.cshtml");
		}

		public ActionResult ExportProviderSettings()
		{
			return PartialView("~/Views/IntegrationPoints/ExportProviderSettings.cshtml");
		}

		public ActionResult ConfigurationDetail()
		{
			return PartialView("~/Views/IntegrationPoints/_Configuration.cshtml");
		}

		public ActionResult LDAPConfiguration()
		{
			return View("~/Views/IntegrationPoints/LDAPConfiguration.cshtml", "_StepLayout");
		}

		public ActionResult RelativityProviderConfiguration()
		{
			return View("~/Views/IntegrationPoints/RelativityProviderConfiguration.cshtml", "_StepLayout");
		}
	}
}