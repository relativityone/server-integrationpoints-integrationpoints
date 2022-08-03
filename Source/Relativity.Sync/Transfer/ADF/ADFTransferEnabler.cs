using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Transfer.ADF
{
    internal class ADFTransferEnabler : IADFTransferEnabler
    {
        private readonly IADLSMigrationStatus _adlsMigrationStatus;
        private readonly ISyncToggles _syncToggles;
        private readonly IInstanceSettings _instanceSettings;
        private readonly IAPILog _logger;

        private bool? _isAdfTransferEnabled;

        public ADFTransferEnabler(IADLSMigrationStatus adlsMigrationStatus, ISyncToggles syncToggles, IInstanceSettings instanceSettings, IAPILog logger)
        {
            _adlsMigrationStatus = adlsMigrationStatus;
            _syncToggles = syncToggles;
            _instanceSettings = instanceSettings;
            _logger = logger;
        }

        public bool IsAdfTransferEnabled
        {
            get
            {
                if (!_isAdfTransferEnabled.HasValue)
                {
                    _isAdfTransferEnabled = ShouldUseADFTransferAsync().GetAwaiter().GetResult();
                }

                return _isAdfTransferEnabled.Value;
            }
        }

        private async Task<bool> ShouldUseADFTransferAsync()
        {
            _logger.LogInformation("Checking if should use ADF to transfer files");

            bool isToggleFMSEnabled = _syncToggles.IsEnabled<UseFMS>();
            _logger.LogInformation("Toggle {toggleName} status: {toggleValue}", typeof(UseFMS).Name, isToggleFMSEnabled);

            bool isTenantFullyMigrated = await _adlsMigrationStatus.IsTenantFullyMigratedAsync().ConfigureAwait(false);
            _logger.LogInformation("Is tenant fully migrated to ADLS: {migrationStatus}", isTenantFullyMigrated);

            bool shouldForceADFTransferAsync = await _instanceSettings.GetShouldForceADFTransferAsync().ConfigureAwait(false);
            _logger.LogInformation("Instance Setting shouldForceADFTransferAsync: {settingValue}", shouldForceADFTransferAsync);

            bool shouldUseADFTransfer = (isToggleFMSEnabled && isTenantFullyMigrated) || shouldForceADFTransferAsync;
            _logger.LogInformation("Should use ADF to transfer files: {shouldForceADFTransferAsync}", shouldForceADFTransferAsync);

            _isAdfTransferEnabled = shouldUseADFTransfer;

            return shouldUseADFTransfer;
        }
    }
}