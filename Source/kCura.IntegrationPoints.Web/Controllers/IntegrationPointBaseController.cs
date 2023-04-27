using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Models;
using System;
using System.Web.Mvc;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Web.Context.UserContext;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public abstract class IntegrationPointBaseController : Controller
	{
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ITabService _tabService;

		private readonly IWorkspaceContext _workspaceIdProvider;
		private readonly IUserContext _userContext;
		private readonly IAPILog _logger;

		protected IntegrationPointBaseController(
			IObjectTypeRepository objectTypeRepository,
			IRepositoryFactory repositoryFactory,
			ITabService tabService,
			IWorkspaceContext workspaceIdProvider,
			IUserContext userContext)
		{
			_objectTypeRepository = objectTypeRepository;
			_repositoryFactory = repositoryFactory;
			_tabService = tabService;
			_workspaceIdProvider = workspaceIdProvider;
			_userContext = userContext;
		}

		protected abstract string ObjectTypeGuid { get; }
		protected abstract string ObjectType { get; }
		protected abstract string APIControllerName { get; }

		public ActionResult Edit(int? artifactId)
		{
			string previousURL = string.Empty;
			try
			{
				_logger.LogInformation("Inside IntegrationPointBaseController, Edit method ");
				int workspaceID = _workspaceIdProvider.GetWorkspaceID();
				_logger.LogInformation("workspaceID " + workspaceID);

				int objectTypeID = _objectTypeRepository.GetObjectTypeID(ObjectType);
				_logger.LogInformation("objectTypeID " + objectTypeID);

				int tabID = _tabService.GetTabId(workspaceID, objectTypeID);
				_logger.LogInformation("tabID " + tabID);

				int objectID = _objectTypeRepository.GetObjectType(objectTypeID).ParentArtifactId;
				_logger.LogInformation("objectID " + objectID);

				previousURL = $"List.aspx?AppID={workspaceID}&ArtifactID={objectID}&ArtifactTypeID={objectTypeID}&SelectedTab={tabID}";
				if (HasPermissions(artifactId))
				{
					_logger.LogWarning("HasPermissions " + artifactId);
					return View("~/Views/IntegrationPoints/Edit.cshtml", new EditPoint
					{
						AppID = workspaceID,
						ArtifactID = artifactId.GetValueOrDefault(0),
						UserID = _userContext.GetUserID(),
						CaseUserID = _userContext.GetWorkspaceUserID(),
						URL = previousURL,
						APIControllerName = APIControllerName,
						ArtifactTypeName = ObjectType
					});
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "IntegrationPointBaseController, Edit error " + ex.Message, ex.InnerException);
			}
			_logger.LogInformation("HasPermissions false");
			return View("~/Views/IntegrationPoints/NotEnoughPermission.cshtml", new EditPoint { URL = previousURL });
		}

		protected bool HasPermissions(int? artifactId)
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(_workspaceIdProvider.GetWorkspaceID());
			bool canImport = permissionRepository.UserCanImport();
			bool canAddOrEdit = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuid),
				artifactId.HasValue ? ArtifactPermission.Edit : ArtifactPermission.Create);
			bool canEditExistingIp = !artifactId.HasValue ||
									permissionRepository.UserHasArtifactInstancePermission(new Guid(ObjectTypeGuid), artifactId.Value, ArtifactPermission.Edit);
			return canImport && canAddOrEdit && canEditExistingIp;
		}
		
		public ActionResult Details(int id)
		{
			IntegrationPointModelBase integrationViewModel = GetIntegrationPointBaseModel(id);

			var model = new IpDetailModel { DataModel = integrationViewModel };

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