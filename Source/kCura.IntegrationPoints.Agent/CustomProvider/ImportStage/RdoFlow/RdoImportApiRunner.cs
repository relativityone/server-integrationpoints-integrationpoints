using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <inheritdoc/>
    internal class RdoImportApiRunner : IImportApiRunner
    {
        private readonly IRdoImportSettingsBuilder _importSettingsBuilder;
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IAPILog _logger;

        /// <summary>
        /// Parameterless constructor for tests purposes only.
        /// </summary>
        public RdoImportApiRunner()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentImportApiRunner"/> class.
        /// </summary>
        /// <param name="importSettingsBuilder">The builder able to create desired ImportAPI configuration.></param>
        /// <param name="serviceFactory">Factory for creating keppler services.</param>
        /// <param name="logger">The logger.</param>
        public RdoImportApiRunner(IRdoImportSettingsBuilder importSettingsBuilder, IKeplerServiceFactory serviceFactory, IAPILog logger)
        {
            _importSettingsBuilder = importSettingsBuilder;
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RunImportJobAsync(ImportJobContext importJobContext, ImportSettings destinationConfiguration, List<IndexedFieldMap> fieldMappings)
        {
            RdoImportConfiguration configuration = await _importSettingsBuilder.BuildAsync(destinationConfiguration, fieldMappings).ConfigureAwait(false);

            Response createResponse = await CreateImportJobAsync(importJobContext).ConfigureAwait(false);
            createResponse.ValidateOrThrow();

            Response docConfigurationResponse = await AttachImportSettingsToImportJobAsync(importJobContext, configuration.RdoSettings).ConfigureAwait(false);
            docConfigurationResponse.ValidateOrThrow();

            //Response advancedResponse = await AttachAdvancedImportSettingsToImportJobAsync(importJobContext, configuration.AdvancedSettings).ConfigureAwait(false);
            //advancedResponse.ValidateOrThrow();

            Response startResponse = await BeginImportJobAsync(importJobContext).ConfigureAwait(false);
            startResponse.ValidateOrThrow();
        }

        private async Task<Response> CreateImportJobAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Creating ImportJob with ID: {importJobId}", importJobContext.ImportJobId);

                return await importJobController.CreateAsync(
                        workspaceID: importJobContext.DestinationWorkspaceId,
                        importJobID: importJobContext.ImportJobId,
                        applicationName: Core.Constants.IntegrationPoints.APPLICATION_NAME,
                        correlationID: importJobContext.RipJobId.ToString())
                    .ConfigureAwait(false);
            }
        }

        private async Task<Response> AttachImportSettingsToImportJobAsync(ImportJobContext importJobContext, ImportRdoSettings importSettings)
        {
            using (IRDOConfigurationController rdoConfigurationController = await _serviceFactory.CreateProxyAsync<IRDOConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Attaching ImportRdoSettings to ImportJob {jobId}... - ImportRdoSettings: {@importRdoSettings}", importJobContext.ImportJobId, importSettings);

                return await rdoConfigurationController.CreateAsync(
                    importJobContext.DestinationWorkspaceId,
                    importJobContext.ImportJobId,
                    importSettings).ConfigureAwait(false);
            }
        }

        private async Task<Response> AttachAdvancedImportSettingsToImportJobAsync(ImportJobContext importJobContext, AdvancedImportSettings importSettings)
        {
            using (IAdvancedConfigurationController configurationController = await _serviceFactory.CreateProxyAsync<IAdvancedConfigurationController>().ConfigureAwait(false))
            {
                _logger.LogInformation(
                    "Attaching AdvancedImportSettings to ImportJob {jobId}... - AdvancedImportSettings: {@importSettings}",
                    importJobContext.ImportJobId,
                    importSettings);

                return await configurationController.CreateAsync(
                        importJobContext.DestinationWorkspaceId,
                        importJobContext.ImportJobId,
                        importSettings)
                    .ConfigureAwait(false);
            }
        }

        private async Task<Response> BeginImportJobAsync(ImportJobContext importJobContext)
        {
            using (IImportJobController importJobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                _logger.LogInformation("Starting ImportJob {jobId} and wait for DataSources...", importJobContext.ImportJobId);

                return await importJobController.BeginAsync(
                        importJobContext.DestinationWorkspaceId,
                        importJobContext.ImportJobId)
                    .ConfigureAwait(false);
            }
        }
    }
}
