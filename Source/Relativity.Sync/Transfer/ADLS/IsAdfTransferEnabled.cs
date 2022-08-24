using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Transfer.ADLS
{
    internal class IsAdfTransferEnabled : IIsAdfTransferEnabled
    {
        private readonly IAdlsMigrationStatus _adlsMigrationStatus;
        private readonly ISyncToggles _syncToggles;
        private readonly IInstanceSettings _instanceSettings;
        private readonly IDocumentSynchronizationConfiguration _documentSynchronizationConfiguration;
        private readonly IAPILog _logger;

        private bool? _isAdfTransferEnabled;

        public IsAdfTransferEnabled(IAdlsMigrationStatus adlsMigrationStatus, ISyncToggles syncToggles, IInstanceSettings instanceSettings, IDocumentSynchronizationConfiguration documentSynchronizationConfiguration, IAPILog logger)
        {
            _adlsMigrationStatus = adlsMigrationStatus;
            _syncToggles = syncToggles;
            _instanceSettings = instanceSettings;
            _documentSynchronizationConfiguration = documentSynchronizationConfiguration;
            _logger = logger;
        }

        public bool Value
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

            bool isToggleFMSEnabled = _syncToggles.IsEnabled<UseFmsToggle>();
            _logger.LogInformation("Toggle {toggleName} status: {toggleValue}", typeof(UseFmsToggle).Name, isToggleFMSEnabled);

            bool isTenantFullyMigrated = await _adlsMigrationStatus.IsTenantFullyMigratedAsync().ConfigureAwait(false);
            _logger.LogInformation("Is tenant fully migrated to ADLS: {migrationStatus}", isTenantFullyMigrated);

            bool shouldForceADFTransferAsync = await _instanceSettings.GetShouldForceADFTransferAsync().ConfigureAwait(false);
            _logger.LogInformation("Instance Setting shouldForceADFTransferAsync: {settingValue}", shouldForceADFTransferAsync);

            bool nativesTransferEnabled = _documentSynchronizationConfiguration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.CopyFiles;

            bool shouldUseADFTransfer = (isToggleFMSEnabled && isTenantFullyMigrated && nativesTransferEnabled) || shouldForceADFTransferAsync;
            _logger.LogInformation("Should use ADF to transfer files: {shouldForceADFTransferAsync}", shouldForceADFTransferAsync);

            _isAdfTransferEnabled = shouldUseADFTransfer;

            return shouldUseADFTransfer;
        }
    }
}
