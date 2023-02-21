﻿using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class RdoImportApiRunner : IImportApiRunner
    {
        private readonly IRdoImportSettingsBuilder _importSettingsBuilder;
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IAPILog _logger;

        public RdoImportApiRunner(IRdoImportSettingsBuilder importSettingsBuilder, IKeplerServiceFactory serviceFactory, IAPILog logger)
        {
            _importSettingsBuilder = importSettingsBuilder;
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public async Task RunImportJobAsync(ImportJobContext importJobContext, string destinationConfiguration, List<FieldMapWrapper> fieldMappings)
        {
            RdoImportConfiguration configuration = await _importSettingsBuilder.BuildAsync(destinationConfiguration, fieldMappings).ConfigureAwait(false);

            Response createResponse = await CreateImportJobAsync(importJobContext).ConfigureAwait(false);
            ValidateOrThrow(createResponse);

            Response docConfigurationResponse = await AttachImportSettingsToImportJobAsync(importJobContext, configuration.RdoSettings).ConfigureAwait(false);
            ValidateOrThrow(docConfigurationResponse);

            Response advancedResponse = await AttachAdvancedImportSettingsToImportJobAsync(importJobContext, configuration.AdvancedSettings).ConfigureAwait(false);
            ValidateOrThrow(advancedResponse);

            Response startResponse = await BeginImportJobAsync(importJobContext).ConfigureAwait(false);
            ValidateOrThrow(startResponse);
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

        private void ValidateOrThrow(Response response)
        {
            if (!response.IsSuccess)
            {
                throw new ImportApiResponseException(response);
            }
        }
    }
}
