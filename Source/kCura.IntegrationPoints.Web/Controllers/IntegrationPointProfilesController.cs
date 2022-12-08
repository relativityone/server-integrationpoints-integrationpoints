using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Extensions;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class IntegrationPointProfilesController : IntegrationPointBaseController
    {
        private readonly IIntegrationPointProfileService _profileService;
        private readonly ICamelCaseSerializer _serializer;

        public IntegrationPointProfilesController(
            IObjectTypeRepository objectTypeRepository,
            IRepositoryFactory repositoryFactory,
            ITabService tabService,
            IIntegrationPointProfileService profileService,
            IWorkspaceContext workspaceIdProvider,
            IUserContext userContext,
            ICamelCaseSerializer serializer
        ) : base(
            objectTypeRepository,
            repositoryFactory,
            tabService,
            workspaceIdProvider,
            userContext
        )
        {
            _profileService = profileService;
            _serializer = serializer;
        }

        protected override string ObjectTypeGuid => ObjectTypeGuids.IntegrationPointProfile;
        protected override string ObjectType => ObjectTypes.IntegrationPointProfile;
        protected override string APIControllerName => Core.Constants.IntegrationPointProfiles.API_CONTROLLER_NAME;

        protected override IntegrationPointWebModelBase GetIntegrationPoint(int id)
        {
            return _profileService.Read(id).ToWebModel(_serializer);
        }
    }
}
