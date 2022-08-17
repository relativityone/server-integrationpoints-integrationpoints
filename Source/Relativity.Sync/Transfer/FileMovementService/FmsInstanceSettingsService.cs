using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.FileMovementService
{
    internal class FmsInstanceSettingsService : IFmsInstanceSettingsService
    {
        private const string RelativityCoreInstanceSettingsSection = "Relativity.Core";
        private const string KubernetesServicesUrlInstanceSettingName = "KubernetesServicesURL";
        private const string FileMigratorInstanceSettingsSection = "Relativity.FileMigrator";
        private const string FileMovementInstanceSettingName = "FileMovementServicesURL";

        private readonly IInstanceSettings _instanceSettings;
        private readonly string fileMovementUrlPart = @"datatransfer-filemovement-api";

        public FmsInstanceSettingsService(IInstanceSettings instanceSettings)
        {
            _instanceSettings = instanceSettings;
        }

        public async Task<string> GetKubernetesServicesUrl()
        {
            string value = await _instanceSettings.GetAsync(KubernetesServicesUrlInstanceSettingName, RelativityCoreInstanceSettingsSection, string.Empty);

            if (string.IsNullOrEmpty(value))
            {
                throw new SyncException($"Instance Setting value is not set: {KubernetesServicesUrlInstanceSettingName}");
            }

            return value;
        }

        public async Task<string> GetFileMovementServiceUrl()
        {
            string value = await _instanceSettings.GetAsync(FileMovementInstanceSettingName, FileMigratorInstanceSettingsSection, string.Empty);

            if (string.IsNullOrEmpty(value))
            {
                value = fileMovementUrlPart;
            }

            return value;
        }
    }
}
