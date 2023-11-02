using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow
{
    /// <inheritdoc/>
    internal class RdoImportApiRunner : IImportApiRunner
    {
        private readonly IRdoImportSettingsBuilder _importSettingsBuilder;
        private readonly IImportApiService _importApiService;
        private readonly IEntityFullNameService _entityFullNameService;
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
        /// <param name="importApiService">The service responsible for ImportAPI calls.</param>
        /// <param name="logger">The logger.</param>
        public RdoImportApiRunner(
            IRdoImportSettingsBuilder importSettingsBuilder,
            IImportApiService importApiService,
            IEntityFullNameService entityFullNameService,
            IAPILog logger)
        {
            _importSettingsBuilder = importSettingsBuilder;
            _importApiService = importApiService;
            _entityFullNameService = entityFullNameService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RunImportJobAsync(ImportJobContext importJobContext, IntegrationPointInfo integrationPoint)
        {
            _logger.LogInformation("ImportApiRunner for RDO flow started. ImportJobId: {jobId}", importJobContext.JobHistoryGuid);

            RdoImportConfiguration configuration = await CreateConfigurationAsync(integrationPoint).ConfigureAwait(false);

            await _importApiService.CreateImportJobAsync(importJobContext).ConfigureAwait(false);

            await _importApiService.ConfigureRdoImportApiJobAsync(importJobContext, configuration).ConfigureAwait(false);

            await _importApiService.StartImportJobAsync(importJobContext).ConfigureAwait(false);
        }

        private async Task<RdoImportConfiguration> CreateConfigurationAsync(IntegrationPointInfo integrationPoint)
        {
            IndexedFieldMap overlyIdentifierField;

            if (await _entityFullNameService.ShouldHandleFullNameAsync(integrationPoint.DestinationConfiguration, integrationPoint.FieldMap).ConfigureAwait(false))
            {
                await _entityFullNameService.EnrichFieldMapWithFullNameAsync(integrationPoint.FieldMap, integrationPoint.DestinationConfiguration.CaseArtifactId).ConfigureAwait(false);
                overlyIdentifierField = integrationPoint.FieldMap.FirstOrDefault(x => x.DestinationFieldName == EntityFieldNames.FullName);
            }
            else
            {
                overlyIdentifierField = integrationPoint.FieldMap.FirstOrDefault(x => x.DestinationFieldName == integrationPoint.DestinationConfiguration.OverlayIdentifier);
            }

            _logger.LogInformation("Configuring import job with fields mapping: {@fieldMap}", integrationPoint.FieldMap);

            RdoImportConfiguration configuration = _importSettingsBuilder.Build(integrationPoint.DestinationConfiguration, integrationPoint.FieldMap, overlyIdentifierField);

            return configuration;
        }
    }
}
