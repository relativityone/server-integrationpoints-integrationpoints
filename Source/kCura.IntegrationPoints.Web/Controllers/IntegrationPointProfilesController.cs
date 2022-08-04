using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Context.UserContext;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class IntegrationPointProfilesController : IntegrationPointBaseController
    {
        private readonly IIntegrationPointProfileService _profileService;

        public IntegrationPointProfilesController(
            IObjectTypeRepository objectTypeRepository,
            IRepositoryFactory repositoryFactory,
            ITabService tabService,
            IIntegrationPointProfileService profileService,
            IWorkspaceContext workspaceIdProvider,
            IUserContext userContext
        ) : base(
            objectTypeRepository,
            repositoryFactory,
            tabService,
            workspaceIdProvider,
            userContext
        )
        {
            _profileService = profileService;
        }

        protected override string ObjectTypeGuid => ObjectTypeGuids.IntegrationPointProfile;
        protected override string ObjectType => ObjectTypes.IntegrationPointProfile;
        protected override string APIControllerName => Core.Constants.IntegrationPointProfiles.API_CONTROLLER_NAME;

        protected override IntegrationPointModelBase GetIntegrationPointBaseModel(int id)
        {
            return _profileService.ReadIntegrationPointProfileModel(id);
        }
    }
}