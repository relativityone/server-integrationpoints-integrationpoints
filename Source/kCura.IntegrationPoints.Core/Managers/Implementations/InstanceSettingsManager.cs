using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class InstanceSettingsManager : IInstanceSettingsManager
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public InstanceSettingsManager(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public string RetriveCurrentInstanceFriendlyName()
        {
            IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
            return instanceSettingRepository.GetConfigurationValue(
                Constants.InstanceSettings.RELATIVITY_AUTHENTICATION_SECTION, Constants.InstanceSettings.FRIENDLY_INSTANCE_NAME);
        }

        public bool RetrieveAllowNoSnapshotImport()
        {
            IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
            string allowNoSnapshotImportSetting = instanceSettingRepository.GetConfigurationValue(
                Constants.InstanceSettings.RELATIVITY_CORE_SECTION, Constants.InstanceSettings.ALLOW_NO_SNAPSHOT_IMPORT);
            if (allowNoSnapshotImportSetting == null)
            {
                return false;
            }
            return bool.TryParse(allowNoSnapshotImportSetting, out bool allowNoSnapshotImport) && allowNoSnapshotImport;
        }

        public bool RetrieveRestrictReferentialFileLinksOnImport()
        {
            IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
            string restrictReferentialFileLinksSetting = instanceSettingRepository.GetConfigurationValue(
                Constants.InstanceSettings.RELATIVITY_CORE_SECTION, Constants.InstanceSettings.RESTRICT_REFERENTIAL_FILE_LINKS_ON_IMPORT);
            if (restrictReferentialFileLinksSetting == null)
            {
                return false;
            }
            return bool.TryParse(restrictReferentialFileLinksSetting, out bool restrictReferentialFileLinks) && restrictReferentialFileLinks;
        }

        public string RetrieveBlockedIPs()
        {
            IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
            string blockedIPs = instanceSettingRepository.GetConfigurationValue(
                Constants.InstanceSettings.INTEGRATION_POINTS_SECTION, Constants.InstanceSettings.BLOCKED_HOSTS);
            return blockedIPs;
        }

        public TimeSpan GetDrainStopTimeout()
        {
            IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
            string drainStopTimeout = instanceSettingRepository.GetConfigurationValue(
                Constants.InstanceSettings.INTEGRATION_POINTS_SECTION, Constants.InstanceSettings.DRAIN_STOP_TIMEOUT);

            if (int.TryParse(drainStopTimeout, out int drainStopTimeoutParsed))
            {
                return TimeSpan.FromSeconds(drainStopTimeoutParsed);
            }
            else
            {
                return TimeSpan.FromMinutes(3);
            }
        }

        public string GetWorkloadSizeSettings()
        {
            IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
            string workloadSizeSettings = instanceSettingRepository.GetConfigurationValue(
                Constants.InstanceSettings.INTEGRATION_POINTS_SECTION, Constants.InstanceSettings.WORKLOAD_SIZE_SETTINGS);
            return workloadSizeSettings;
        }
    }
}