using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class OAuthClientManager : IOAuthClientManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public OAuthClientManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public OAuthClientDto RetrieveOAuthClientForFederatedInstance(int federatedInstanceArtifactId)
		{
			IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
			string clientId = instanceSettingRepository.GetConfigurationValue(kCura.IntegrationPoints.Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION, "Client Id");
			string clientSecret = instanceSettingRepository.GetConfigurationValue(kCura.IntegrationPoints.Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION, "Client Secret");

			var oAuthClientDto = new OAuthClientDto()
			{
				ClientId = clientId,
				ClientSecret = clientSecret
			};

			return oAuthClientDto;
		}
	}
}