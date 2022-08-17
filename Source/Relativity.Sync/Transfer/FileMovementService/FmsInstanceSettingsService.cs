using System;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.Sync.Transfer.FileMovementService
{
    internal class FmsInstanceSettingsService : IFmsInstanceSettingsService
    {
        private const string RelativityCoreInstanceSettingsSection = "Relativity.Core";
        private const string KubernetesServicesUrlInstanceSettingName = "KubernetesServicesURL";
        private const string FileMigratorInstanceSettingsSection = "Relativity.FileMigrator";
        private const string FileMovementInstanceSettingName = "FileMovementServicesURL";

        private readonly string fileMovementUrlPart = @"datatransfer-filemovement-api";
        private readonly IHelper _helper;

        public FmsInstanceSettingsService(IHelper helper)
        {
            _helper = helper;
        }

        public async Task<string> GetKubernetesServicesUrl()
        {
            IInstanceSettingsBundle instanceSettingBundle = _helper.GetInstanceSettingBundle();
            string value = await instanceSettingBundle.GetStringAsync(RelativityCoreInstanceSettingsSection, KubernetesServicesUrlInstanceSettingName);
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(KubernetesServicesUrlInstanceSettingName);
            }

            return value;
        }

        public async Task<string> GetFileMovementServiceUrl()
        {
            IInstanceSettingsBundle instanceSettingBundle = _helper.GetInstanceSettingBundle();
            string value = await instanceSettingBundle.GetStringAsync(FileMigratorInstanceSettingsSection, FileMovementInstanceSettingName);
            if (string.IsNullOrEmpty(value))
            {
                value = fileMovementUrlPart;
            }

            return value;
        }
    }
}
