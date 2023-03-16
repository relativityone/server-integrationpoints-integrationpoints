using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    internal class ImportService : IImportService
    {
        private readonly IDestinationServiceFactoryForUser _serviceFactoryForUser;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IImportServiceConfiguration _configuration;
        private readonly IAPILog _logger;

        public ImportService(
            IDestinationServiceFactoryForUser serviceFactoryForUser,
            ISourceServiceFactoryForAdmin serviceFacotryForAdmin,
            IImportServiceConfiguration configuration,
            IAPILog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _serviceFactoryForAdmin = serviceFacotryForAdmin;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task CreateImportJobAsync(SyncJobParameters parameters)
        {
            using (IImportJobController importJobController = await _serviceFactoryForUser.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Creating ImportJob with ID: {importJobId}", _configuration.ExportRunId);

                (await importJobController.CreateAsync(
                       workspaceID: _configuration.DestinationWorkspaceArtifactId,
                       importJobID: _configuration.ExportRunId,
                       applicationName: parameters.SyncApplicationName,
                       correlationID: parameters.WorkflowId)
                    .ConfigureAwait(false))
                    .Validate($"{nameof(ImportService.CreateImportJobAsync)} failed.");
            }
        }

        public async Task ConfigureDocumentImportSettingsAsync(ImportSettings settings)
        {
            await AttachImportSettingsToImportJobAsync(settings.DocumentSettings).ConfigureAwait(false);
            await AttachAdvancedImportSettingsToImportJobAsync(settings.AdvancedSettings).ConfigureAwait(false);
        }

        public async Task BeginImportJobAsync()
        {
            using (IImportJobController importJobController = await _serviceFactoryForUser.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Starting ImportJob {jobId} and wait for DataSources...", _configuration.ExportRunId);

                (await importJobController.BeginAsync(
                       _configuration.DestinationWorkspaceArtifactId,
                       _configuration.ExportRunId)
                    .ConfigureAwait(false))
                    .Validate($"{nameof(ImportService.BeginImportJobAsync)} failed.");
            }
        }

        public async Task AddDataSourceAsync(Guid batchGuid, DataSourceSettings settings)
        {
            using (IImportSourceController importSource = await _serviceFactoryForUser.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                (await importSource.AddSourceAsync(
                       _configuration.DestinationWorkspaceArtifactId,
                       _configuration.ExportRunId,
                       batchGuid,
                       settings)
                    .ConfigureAwait(false))
                    .Validate($"{nameof(ImportService.AddDataSourceAsync)} failed.");
            }
        }

        public async Task CancelJobAsync()
        {
            using (IImportJobController importJobController = await _serviceFactoryForUser.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Canceling job {jobId}...", _configuration.ExportRunId);

                (await importJobController.CancelAsync(
                      _configuration.DestinationWorkspaceArtifactId,
                      _configuration.ExportRunId)
                    .ConfigureAwait(false))
                    .Validate($"{nameof(ImportService.CancelJobAsync)} failed.");
            }
        }

        public async Task EndJobAsync()
        {
            using (IImportJobController importJobController = await _serviceFactoryForUser.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Ending job {jobId}...", _configuration.ExportRunId);

                (await importJobController.EndAsync(
                      _configuration.DestinationWorkspaceArtifactId,
                      _configuration.ExportRunId)
                    .ConfigureAwait(false))
                    .Validate($"{nameof(ImportService.EndJobAsync)} failed.");
            }
        }

        public async Task<ImportProgress> GetJobImportProgressValueAsync()
        {
            using (IImportJobController job = await _serviceFactoryForAdmin.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                return (await job.GetProgressAsync(
                        _configuration.DestinationWorkspaceArtifactId,
                        _configuration.ExportRunId)
                    .ConfigureAwait(false))
                    .UnwrapOrThrow($"{nameof(ImportService.GetJobImportProgressValueAsync)} failed.");
            }
        }

        public async Task<ImportDetails> GetJobImportStatusAsync()
        {
            using (IImportJobController job = await _serviceFactoryForUser.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                return (await job.GetDetailsAsync(
                    _configuration.DestinationWorkspaceArtifactId,
                    _configuration.ExportRunId)
                .ConfigureAwait(false))
                .UnwrapOrThrow($"{nameof(ImportService.GetJobImportStatusAsync)} failed.");
            }
        }

        public async Task<ImportErrors> GetDataSourceErrorsAsync(Guid dataSourceId, int start, int length)
        {
            using (IImportSourceController importSource = await _serviceFactoryForUser.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                return (await importSource.GetItemErrorsAsync(
                           _configuration.DestinationWorkspaceArtifactId,
                           _configuration.ExportRunId,
                           dataSourceId,
                           start,
                           length)
                       .ConfigureAwait(false))
                       .UnwrapOrThrow($"{nameof(ImportService.GetDataSourceErrorsAsync)} failed.");
            }
        }

        public async Task<DataSourceDetails> GetDataSourceStatusAsync(Guid dataSourceGuid)
        {
            using (IImportSourceController importSource = await _serviceFactoryForUser.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                return (await importSource.GetDetailsAsync(
                               _configuration.DestinationWorkspaceArtifactId,
                               _configuration.ExportRunId,
                               dataSourceGuid)
                           .ConfigureAwait(false))
                           .UnwrapOrThrow($"{nameof(ImportService.GetDataSourceStatusAsync)} failed.");
            }
        }

        public async Task<ImportProgress> GetDataSourceProgressAsync(Guid dataSourceGuid)
        {
            using (IImportSourceController importSource = await _serviceFactoryForUser.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                return (await importSource.GetProgressAsync(
                            _configuration.DestinationWorkspaceArtifactId,
                            _configuration.ExportRunId,
                            dataSourceGuid)
                        .ConfigureAwait(false))
                        .UnwrapOrThrow($"{nameof(GetDataSourceProgressAsync)} failed.");
            }
        }

        private async Task AttachImportSettingsToImportJobAsync(ImportDocumentSettings importSettings)
        {
            using (IDocumentConfigurationController documentConfigurationController = await _serviceFactoryForUser.CreateProxyAsync<IDocumentConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Attaching ImportDocumentsSettings to ImportJob {jobId}... - DocumentImportSettings: {@documentSettings}", _configuration.ExportRunId, importSettings);

                (await documentConfigurationController.CreateAsync(
                    _configuration.DestinationWorkspaceArtifactId,
                    _configuration.ExportRunId,
                    importSettings)
                    .ConfigureAwait(false))
                    .Validate($"{nameof(ImportService.AttachImportSettingsToImportJobAsync)} failed.");
            }
        }

        private async Task AttachAdvancedImportSettingsToImportJobAsync(AdvancedImportSettings importSettings)
        {
            using (IAdvancedConfigurationController configurationController = await _serviceFactoryForUser.CreateProxyAsync<IAdvancedConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching AdvancedImportSettings to ImportJob {jobId}... - AdvancedImportSettings: {@importSettings}",
                    _configuration.ExportRunId,
                    importSettings);

                (await configurationController.CreateAsync(
                       _configuration.DestinationWorkspaceArtifactId,
                       _configuration.ExportRunId,
                       importSettings)
                    .ConfigureAwait(false))
                    .Validate($"{nameof(ImportService.AttachAdvancedImportSettingsToImportJobAsync)} failed.");
            }
        }
    }
}
