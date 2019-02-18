using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.LDAPProvider;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : IntegrationPointBaseController
	{
		private readonly IIntegrationPointService _integrationPointService;

		public IntegrationPointsController(
			IObjectTypeRepository objectTypeRepository, 
			IRepositoryFactory repositoryFactory,
			ITabService tabService, 
			ILDAPServiceFactory ldapServiceFactory,
			IIntegrationPointService integrationPointService,
			IWorkspaceIdProvider workspaceIdProvider
		) : base(
			objectTypeRepository, 
			repositoryFactory, 
			tabService,
			ldapServiceFactory,
			workspaceIdProvider
		)
		{
			_integrationPointService = integrationPointService;
		}

		protected override string ObjectTypeGuid => ObjectTypeGuids.IntegrationPoint;
		protected override string ObjectType => ObjectTypes.IntegrationPoint;
		protected override string APIControllerName => Core.Constants.IntegrationPoints.API_CONTROLLER_NAME;

		protected override IntegrationPointModelBase GetIntegrationPointBaseModel(int id)
		{
			return _integrationPointService.ReadIntegrationPoint(id);
		}

		public ActionResult SaveAsProfileModal()
		{
			return PartialView();
		}
	}
}