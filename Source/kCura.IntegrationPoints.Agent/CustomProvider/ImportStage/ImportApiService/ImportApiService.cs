using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <inheritdoc/>
    internal class ImportApiService : IImportApiService
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IAPILog _logger;

        /// <summary>
        /// Parameterless constructor for tests purposes only.
        /// </summary>
        public ImportApiService()
        {
        }

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
                _logger.LogInformation("Creating ImportJob with ID: {importJobId}", importJobContext.ImportJobId);

                Response response = await importJobController.CreateAsync(
                        workspaceID: importJobContext.DestinationWorkspaceId,
                        importJobID: importJobContext.ImportJobId,
                        applicationName: Core.Constants.IntegrationPoints.APPLICATION_NAME,
                        correlationID: importJobContext.RipJobId.ToString())
                    .ConfigureAwait(false);

                ValidateOrThrow(response);
            }
        }

        /// <inheritdoc/>
        public async Task StartImportJobAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Starting ImportJob {jobId} and wait for DataSources...", importJobContext.ImportJobId);

                Response response = await importJobController.BeginAsync(
                        importJobContext.DestinationWorkspaceId,
                        importJobContext.ImportJobId)
                    .ConfigureAwait(false);

                ValidateOrThrow(response);
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

        private async Task CreateDocumentConfigurationAsync(ImportJobContext importJobContext, ImportDocumentSettings settings)
        {
            using (IDocumentConfigurationController documentConfigurationController = await _serviceFactory.CreateProxyAsync<IDocumentConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching ImportDocumentsSettings to ImportJob {jobId}... - ImportDocumentSettings: {@settings}",
                    importJobContext.ImportJobId,
                    settings);

                Response response = await documentConfigurationController.CreateAsync(
                        importJobContext.DestinationWorkspaceId,
                        importJobContext.ImportJobId,
                        settings)
                    .ConfigureAwait(false);

                ValidateOrThrow(response);
            }
        }

        private async Task CreateRdoConfigurationAsync(ImportJobContext importJobContext, ImportRdoSettings settings)
        {
            using (IRDOConfigurationController rdoConfigurationController =
                   await _serviceFactory.CreateProxyAsync<IRDOConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching ImportRdoSettings to ImportJob {jobId}... - ImportRdoSettings: {@settings}",
                    importJobContext.ImportJobId,
                    settings);

                Response response = await rdoConfigurationController.CreateAsync(
                        importJobContext.DestinationWorkspaceId,
                        importJobContext.ImportJobId,
                        settings)
                    .ConfigureAwait(false);

                ValidateOrThrow(response);
            }
        }

        private async Task CreateAdvancedConfigurationAsync(ImportJobContext importJobContext, AdvancedImportSettings settings)
        {
            using (IAdvancedConfigurationController configurationController = await _serviceFactory.CreateProxyAsync<IAdvancedConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching AdvancedImportSettings to ImportJob {jobId}... - AdvancedImportSettings: {@settings}",
                    importJobContext.ImportJobId,
                    settings);

                Response response = await configurationController.CreateAsync(
                        importJobContext.DestinationWorkspaceId,
                        importJobContext.ImportJobId,
                        settings)
                    .ConfigureAwait(false);

                ValidateOrThrow(response);
            }
        }

        private static void ValidateOrThrow(Response response)
        {
            if (response?.IsSuccess != true)
            {
                throw new ImportApiResponseException(response);
            }
        }
    }
}
