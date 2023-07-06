using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Utils;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <inheritdoc/>
    internal class ImportApiService : IImportApiService
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IAPILog _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentImportApiRunner"/> class.
        /// </summary>
        /// <param name="serviceFactory">Factory for creating kepler services.</param>
        /// <param name="logger">The logger.</param>
        public ImportApiService(IKeplerServiceFactory serviceFactory, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task CreateImportJobAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Creating ImportJob with ID: {importJobId}", importJobContext.JobHistoryGuid);

                Response response = await importJobController.CreateAsync(
                        workspaceID: importJobContext.WorkspaceId,
                        importJobID: importJobContext.JobHistoryGuid,
                        applicationName: Core.Constants.IntegrationPoints.APPLICATION_NAME,
                        correlationID: importJobContext.RipJobId.ToString())
                    .ConfigureAwait(false);

                response.Validate();
            }
        }

        /// <inheritdoc/>
        public async Task StartImportJobAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Starting ImportJob {jobId} and wait for DataSources...", importJobContext.JobHistoryGuid);

                Response response = await importJobController.BeginAsync(
                        importJobContext.WorkspaceId,
                        importJobContext.JobHistoryGuid)
                    .ConfigureAwait(false);

                response.Validate();
            }
        }

        /// <inheritdoc/>
        public async Task ConfigureDocumentImportApiJobAsync(ImportJobContext importJobContext, DocumentImportConfiguration configuration)
        {
            await CreateDocumentConfigurationAsync(importJobContext, configuration.DocumentSettings).ConfigureAwait(false);

            await CreateAdvancedConfigurationAsync(importJobContext, configuration.AdvancedSettings).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task ConfigureRdoImportApiJobAsync(ImportJobContext importJobContext, RdoImportConfiguration configuration)
        {
            await CreateRdoConfigurationAsync(importJobContext, configuration.RdoSettings).ConfigureAwait(false);

            await CreateAdvancedConfigurationAsync(importJobContext, configuration.AdvancedSettings).ConfigureAwait(false);
        }

        public async Task AddDataSourceAsync(ImportJobContext importJobContext,
            Guid sourceId,
            DataSourceSettings dataSourceSettings)
        {
            using (IImportSourceController importSource = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                (await importSource.AddSourceAsync(
                            importJobContext.WorkspaceId,
                            importJobContext.JobHistoryGuid,
                            sourceId,
                            dataSourceSettings)
                        .ConfigureAwait(false))
                    .Validate($"{nameof(ImportApiService.AddDataSourceAsync)} failed.");
            }
        }

        public async Task CancelJobAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Canceling job {jobId}...", importJobContext.JobHistoryGuid);

                (await importJobController.CancelAsync(
                            importJobContext.WorkspaceId,
                            importJobContext.JobHistoryGuid)
                        .ConfigureAwait(false))
                    .Validate($"{nameof(ImportApiService.CancelJobAsync)} failed.");
            }
        }

        public async Task EndJobAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Ending job {jobId}...", importJobContext.JobHistoryGuid);

                (await importJobController.EndAsync(
                            importJobContext.WorkspaceId,
                            importJobContext.JobHistoryGuid)
                        .ConfigureAwait(false))
                    .Validate($"{nameof(ImportApiService.EndJobAsync)} failed.");
            }
        }

        public async Task<ImportDetails> GetJobImportStatusAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController job = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                return (await job.GetDetailsAsync(
                            importJobContext.WorkspaceId,
                            importJobContext.JobHistoryGuid)
                        .ConfigureAwait(false))
                    .UnwrapOrThrow($"{nameof(ImportApiService.GetJobImportStatusAsync)} failed.");
            }
        }

        public async Task<ImportProgress> GetJobImportProgressValueAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController jobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                ValueResponse<ImportProgress> response = await jobController.GetProgressAsync(importJobContext.WorkspaceId, importJobContext.JobHistoryGuid).ConfigureAwait(false);
                ImportProgress progress = response.UnwrapOrThrow();
                return progress;
            }
        }

        private async Task CreateDocumentConfigurationAsync(ImportJobContext importJobContext, ImportDocumentSettings settings)
        {
            using (IDocumentConfigurationController documentConfigurationController = await _serviceFactory.CreateProxyAsync<IDocumentConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching ImportDocumentsSettings to ImportJob {jobId}... - ImportDocumentSettings: {@settings}",
                    importJobContext.JobHistoryGuid,
                    settings);

                Response response = await documentConfigurationController.CreateAsync(
                        importJobContext.WorkspaceId,
                        importJobContext.JobHistoryGuid,
                        settings)
                    .ConfigureAwait(false);

                response.Validate();
            }
        }

        private async Task CreateRdoConfigurationAsync(ImportJobContext importJobContext, ImportRdoSettings settings)
        {
            using (IRDOConfigurationController rdoConfigurationController =
                   await _serviceFactory.CreateProxyAsync<IRDOConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching ImportRdoSettings to ImportJob {jobId}... - ImportRdoSettings: {@settings}",
                    importJobContext.JobHistoryGuid,
                    settings);

                Response response = await rdoConfigurationController.CreateAsync(
                        importJobContext.WorkspaceId,
                        importJobContext.JobHistoryGuid,
                        settings)
                    .ConfigureAwait(false);

                response.Validate();
            }
        }

        private async Task CreateAdvancedConfigurationAsync(ImportJobContext importJobContext, AdvancedImportSettings settings)
        {
            using (IAdvancedConfigurationController configurationController = await _serviceFactory.CreateProxyAsync<IAdvancedConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching AdvancedImportSettings to ImportJob {jobId}... - AdvancedImportSettings: {@settings}",
                    importJobContext.JobHistoryGuid,
                    settings);

                Response response = await configurationController.CreateAsync(
                        importJobContext.WorkspaceId,
                        importJobContext.JobHistoryGuid,
                        settings)
                    .ConfigureAwait(false);

                response.Validate();
            }
        }

        public async Task<DataSourceDetails> GetDataSourceDetailsAsync(ImportJobContext importJobContext, Guid sourceId)
        {
            using (IImportSourceController importSource = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                ValueResponse<DataSourceDetails> response = await importSource.GetDetailsAsync(importJobContext.WorkspaceId, importJobContext.JobHistoryGuid, sourceId)
                    .ConfigureAwait(false);

                DataSourceDetails details = response.UnwrapOrThrow();
                return details;
            }
        }

        public async Task<ImportErrors> GetDataSourceErrorsAsync(ImportJobContext importJobContext, Guid sourceId, int length)
        {
            using (IImportSourceController importSource = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                ValueResponse<ImportErrors> response = await importSource.GetItemErrorsAsync(importJobContext.WorkspaceId, importJobContext.JobHistoryGuid, sourceId, 0, length)
                    .ConfigureAwait(false);

                ImportErrors errors = response.UnwrapOrThrow();
                return errors;
            }
        }
    }
}
