using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class InstanceSettingsManager : IInstanceSettingsManager 
	{
		private const string _RELATIVITY_AUTHENTICATION_SECTION = "Relativity.Authentication";
		private const string _FRIENDLY_INSTANCE_NAME = "FriendlyInstanceName";

		private const string _RELATIVITY_CORE_SECTION = "Relativity.Core";
		private const string _ALLOW_NO_SNAPSHOT_IMPORT = "AllowNoSnapshotImport";
		private const string _RESTRICT_REFERENTIAL_FILE_LINKS_ON_IMPORT = "RestrictReferentialFileLinksOnImport";

		private const string _INTEGRATION_POINTS_SECTION = "kCura.IntegrationPoints";
		private const string _BLOCKED_HOSTS = "BlockedIPs";

		private readonly IRepositoryFactory _repositoryFactory;

		public InstanceSettingsManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public string RetriveCurrentInstanceFriendlyName()
		{
			IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
			return instanceSettingRepository.GetConfigurationValue(_RELATIVITY_AUTHENTICATION_SECTION, _FRIENDLY_INSTANCE_NAME);
		}

		public bool RetrieveAllowNoSnapshotImport()
		{
			IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
			string allowNoSnapshotImportSetting = instanceSettingRepository.GetConfigurationValue(_RELATIVITY_CORE_SECTION, _ALLOW_NO_SNAPSHOT_IMPORT);
			if (allowNoSnapshotImportSetting == null)
			{
				return false;
			}
			return bool.TryParse(allowNoSnapshotImportSetting, out bool allowNoSnapshotImport) && allowNoSnapshotImport;
		}

		public bool RetrieveRestrictReferentialFileLinksOnImport()
		{
			IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
			string restrictReferentialFileLinksSetting = instanceSettingRepository.GetConfigurationValue(_RELATIVITY_CORE_SECTION, _RESTRICT_REFERENTIAL_FILE_LINKS_ON_IMPORT);
			if (restrictReferentialFileLinksSetting == null)
			{
				return false;
			}
			return bool.TryParse(restrictReferentialFileLinksSetting, out bool restrictReferentialFileLinks) && restrictReferentialFileLinks;
		}

		public string RetrieveBlockedIPs()
		{
			IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
			string blockedIPs = instanceSettingRepository.GetConfigurationValue(_INTEGRATION_POINTS_SECTION, _BLOCKED_HOSTS);
			return blockedIPs;
		}
	}
}