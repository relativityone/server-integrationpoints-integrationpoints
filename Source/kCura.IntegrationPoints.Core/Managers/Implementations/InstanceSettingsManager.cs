using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class InstanceSettingsManager : IInstanceSettingsManager {
		private readonly IRepositoryFactory _repositoryFactory;

		private const string _RELATIVITY_AUTHENTICATION = "Relativity.Authentication";
		private const string _FRIENDLY_INSTANCE_NAME = "FriendlyInstanceName";

		private const string _RELATIVITY_CORE = "Relativity.Core";
		private const string _ALLOW_NO_SNAPSHOT_IMPORT = "AllowNoSnapshotImport";

		public InstanceSettingsManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public string RetriveCurrentInstanceFriendlyName()
		{
			IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
			return instanceSettingRepository.GetConfigurationValue(_RELATIVITY_AUTHENTICATION, _FRIENDLY_INSTANCE_NAME);
		}

		public bool RetrieveAllowNoSnapshotImport()
		{
			IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
			var allowNoSnapshotImportSetting = instanceSettingRepository.GetConfigurationValue(_RELATIVITY_CORE, _ALLOW_NO_SNAPSHOT_IMPORT);
			if (allowNoSnapshotImportSetting == null)
			{
				return false;
			}
			bool allowNoSnapshotImport;
			return bool.TryParse(allowNoSnapshotImportSetting, out allowNoSnapshotImport) && allowNoSnapshotImport;
		}
	}
}