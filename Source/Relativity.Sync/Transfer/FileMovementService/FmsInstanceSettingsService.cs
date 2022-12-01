using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.FileMovementService
{
    internal class FmsInstanceSettingsService : IFmsInstanceSettingsService
    {
        private const string _RELATIVITY_CORE_SETTING_SECTION = "Relativity.Core";
        private const string _KUBERNETES_SERVICES_URL = "KubernetesServicesURL";

        private const string _SYNC_SECTION = "Relativity.Sync";
        private const string _FILE_MOVEMENT_SERVICE_URL = "FileMovementServicesURL";
        private const string _FILE_MOVEMENT_SERVICE_MONITORING_INTERVAL = "FileMovementServiceMonitoringInterval";

        private readonly IInstanceSettings _instanceSettings;
        private readonly string fileMovementUrlPart = @"datatransfer-filemovement-api";

        public FmsInstanceSettingsService(IInstanceSettings instanceSettings)
        {
            _instanceSettings = instanceSettings;
        }

        public async Task<string> GetKubernetesServicesUrl()
        {
            string value = await _instanceSettings.GetAsync(_KUBERNETES_SERVICES_URL, _RELATIVITY_CORE_SETTING_SECTION, string.Empty);

            if (string.IsNullOrEmpty(value))
            {
                throw new SyncException($"Instance Setting value is not set: {_KUBERNETES_SERVICES_URL}");
            }

            return value;
        }

        public async Task<string> GetFileMovementServiceUrl()
        {
            string value = await _instanceSettings.GetAsync(_FILE_MOVEMENT_SERVICE_URL, _SYNC_SECTION, string.Empty);

            if (string.IsNullOrEmpty(value))
            {
                value = fileMovementUrlPart;
            }

            return value;
        }

        public async Task<int> GetMonitoringInterval()
        {
            int value = await _instanceSettings.GetAsync(_FILE_MOVEMENT_SERVICE_MONITORING_INTERVAL, _SYNC_SECTION, 5);
            return value;
        }
    }
}
