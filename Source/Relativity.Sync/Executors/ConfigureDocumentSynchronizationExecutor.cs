using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    internal class ConfigureDocumentSynchronizationExecutor : IExecutor<IConfigureDocumentSynchronizationConfiguration>
    {
        private readonly SyncJobParameters _parameters;
        private readonly IDestinationServiceFactoryForUser _serviceFactory;
        private readonly IImportSettingsBuilder _settingsBuilder;
        private readonly IAPILog _logger;

        public ConfigureDocumentSynchronizationExecutor(
            SyncJobParameters parameters,
            IDestinationServiceFactoryForUser serviceFactory,
            IImportSettingsBuilder settingsBuilder,
            IAPILog logger)
        {
            _parameters = parameters;
            _serviceFactory = serviceFactory;
            _settingsBuilder = settingsBuilder;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IConfigureDocumentSynchronizationConfiguration configuration, CompositeCancellationToken token)
        {
            try
            {
                ImportSettings settings = await _settingsBuilder.BuildAsync(configuration, token.AnyReasonCancellationToken).ConfigureAwait(false);

                ImportContext context = new ImportContext(configuration.ExportRunId, configuration.DestinationWorkspaceArtifactId);

                await CreateImportJobAsync(context).ConfigureAwait(false);

                await AttachImportSettingsToImportJobAsync(context, settings.DocumentSettings).ConfigureAwait(false);

                // Ucomment once REL-774348 will be resolved
                // await AttachAdvancedImportSettingsToImportJobAsync(context, settings.AdvancedSettings).ConfigureAwait(false);
                await BeginImportJobAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured when configuring IAPI2.0 document synchronization");
                return ExecutionResult.Failure(ex);
            }

            return ExecutionResult.Success();
        }

        private async Task CreateImportJobAsync(ImportContext context)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Creating ImportJob with {importJobId} ID...", context.ImportJobId);

                Response response = await importJobController.CreateAsync(
                        workspaceID: context.DestinationWorkspaceId,
                        importJobID: context.ImportJobId,
                        applicationName: _parameters.SyncApplicationName,
                        correlationID: _parameters.WorkflowId)
                    .ConfigureAwait(false);

                ValidateResponse(response);
            }
        }

        private async Task AttachImportSettingsToImportJobAsync(ImportContext context, ImportDocumentSettings importSettings)
        {
            using (IDocumentConfigurationController documentConfigurationController = await _serviceFactory.CreateProxyAsync<IDocumentConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Attaching ImportDocumentsSettings to ImportJob {jobId}...", context.ImportJobId);

                Response response = await documentConfigurationController.CreateAsync(
                    context.DestinationWorkspaceId,
                    context.ImportJobId,
                    importSettings).ConfigureAwait(false);

                ValidateResponse(response);
            }
        }

        private async Task AttachAdvancedImportSettingsToImportJobAsync(ImportContext context, AdvancedImportSettings importSettings)
        {
            using (IAdvancedConfigurationController configurationController = await _serviceFactory.CreateProxyAsync<IAdvancedConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching AdvancedImportSettings to ImportJob {jobId}... - AdvancedImportSettings: {@importSettings}",
                    context.ImportJobId,
                    importSettings);

                Response response = await configurationController.CreateAsync(
                        context.DestinationWorkspaceId,
                        context.ImportJobId,
                        importSettings)
                    .ConfigureAwait(false);

                ValidateResponse(response);
            }
        }

        private async Task BeginImportJobAsync(ImportContext context)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Starting ImportJob {jobId} and wait for DataSources...", context.ImportJobId);

                Response response = await importJobController.BeginAsync(
                        context.DestinationWorkspaceId,
                        context.ImportJobId)
                    .ConfigureAwait(false);

                ValidateResponse(response);
            }
        }

        private void ValidateResponse(Response response)
        {
            if (response.IsSuccess == false)
            {
                string message = $"ImportJobId: {response.ImportJobID}, Error code: {response.ErrorCode}, message: {response.ErrorMessage}";
                throw new SyncException(message);
            }
        }

        private class ImportContext
        {
            public ImportContext(Guid importJobId, int destinationWorkspaceId)
            {
                ImportJobId = importJobId;
                DestinationWorkspaceId = destinationWorkspaceId;
            }

            public Guid ImportJobId { get; }

            public int DestinationWorkspaceId { get; }
        }
    }
}
