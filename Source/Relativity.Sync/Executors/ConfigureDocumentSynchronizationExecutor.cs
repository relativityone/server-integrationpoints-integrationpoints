using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    internal class ConfigureDocumentSynchronizationExecutor : IExecutor<IConfigureDocumentSynchronizationConfiguration>
    {
        private readonly SyncJobParameters _parameters;
        private readonly IImportSettingsBuilder _settingsBuilder;
        private readonly IImportService _importService;
        private readonly IAPILog _logger;

        public ConfigureDocumentSynchronizationExecutor(
            SyncJobParameters parameters,
            IImportSettingsBuilder settingsBuilder,
            IImportService importService,
            IAPILog logger)
        {
            _parameters = parameters;
            _settingsBuilder = settingsBuilder;
            _importService = importService;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IConfigureDocumentSynchronizationConfiguration configuration, CompositeCancellationToken token)
        {
            try
            {
                ImportSettings settings = await _settingsBuilder.BuildAsync(configuration, token.AnyReasonCancellationToken).ConfigureAwait(false);

                await _importService.CreateImportJobAsync(_parameters).ConfigureAwait(false);

                await _importService.ConfigureDocumentImportSettingsAsync(settings).ConfigureAwait(false);

                await _importService.BeginImportJobAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred when configuring IAPI2.0 document synchronization.");
                return ExecutionResult.Failure(ex);
            }

            return ExecutionResult.Success();
        }
    }
}
