using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class InstanceSettingsManager : IInstanceSettingsManager {
		private readonly IRepositoryFactory _repositoryFactory;
		private const string RELATIVITY_AUTHENTICATION = "Relativity.Authentication";
		private const string FRIENDLY_INSTANCE_NAME = "FriendlyInstanceName";

		public InstanceSettingsManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}
		public string RetriveCurrentInstanceFriendlyName()
		{
			IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
			return instanceSettingRepository.GetConfigurationValue(RELATIVITY_AUTHENTICATION, FRIENDLY_INSTANCE_NAME);
		}
	}
}