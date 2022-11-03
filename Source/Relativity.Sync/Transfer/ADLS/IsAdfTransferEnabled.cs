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
            _logger.LogInformation("Checking if should use ADF to transfer files...");
            if (_documentSynchronizationConfiguration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.CopyFiles)
            {
                _logger.LogInformation(
                    "ADF transfer won't be used - NativeFileCopyMode: {nativeFileCopyMode}",
                    _documentSynchronizationConfiguration.ImportNativeFileCopyMode);
                return _isAdfTransferEnabled ??= false;
            }

            bool shouldForceADFTransfer = await _instanceSettings.GetShouldForceADFTransferAsync().ConfigureAwait(false);
            _logger.LogInformation("Instance Setting shouldForceADFTransferAsync: {settingValue}", shouldForceADFTransfer);
            if (shouldForceADFTransfer)
            {
                return _isAdfTransferEnabled ??= true;
            }

            bool isToggleFMSEnabled = _syncToggles.IsEnabled<UseFmsToggle>();
            _logger.LogInformation("Toggle {toggleName} status: {toggleValue}", typeof(UseFmsToggle).Name, isToggleFMSEnabled);
            if (!isToggleFMSEnabled)
            {
                return _isAdfTransferEnabled ??= false;
            }

            bool isTenantFullyMigrated = await _adlsMigrationStatus.IsTenantFullyMigratedAsync().ConfigureAwait(false);
            _logger.LogInformation("Is tenant fully migrated to ADLS: {migrationStatus}", isTenantFullyMigrated);
            if (isTenantFullyMigrated)
            {
                return _isAdfTransferEnabled ??= true;
            }

            return _isAdfTransferEnabled ??= false;
        }
    }
}
