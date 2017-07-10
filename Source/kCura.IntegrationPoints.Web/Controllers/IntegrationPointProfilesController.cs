using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.LDAPProvider;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointProfilesController : IntegrationPointBaseController
	{
		private readonly IIntegrationPointProfileService _profileService;

		public IntegrationPointProfilesController(RSAPIRdoQuery rdoQuery, IRepositoryFactory repositoryFactory, ITabService tabService, ILDAPServiceFactory ldapServiceFactory,

            IIntegrationPointProfileService profileService) : base(rdoQuery, repositoryFactory, tabService, ldapServiceFactory)
		{
			_profileService = profileService;
		}

		protected override string ObjectTypeGuid => ObjectTypeGuids.IntegrationPointProfile;
		protected override string ObjectType => ObjectTypes.IntegrationPointProfile;
		protected override string APIControllerName => Core.Constants.IntegrationPointProfiles.API_CONTROLLER_NAME;

		protected override IntegrationPointModelBase GetIntegrationPointBaseModel(int id)
		{
			return _profileService.ReadIntegrationPointProfile(id);
		}
	}
}