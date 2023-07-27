using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow
{
    /// <inheritdoc/>
    internal class RdoImportApiRunner : IImportApiRunner
    {
        private readonly IRdoImportSettingsBuilder _importSettingsBuilder;
        private readonly IImportApiService _importApiService;
        private readonly IAPILog _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentImportApiRunner"/> class.
        /// </summary>
        /// <param name="importSettingsBuilder">The builder able to create desired ImportAPI configuration.></param>
        /// <param name="importApiService">The service responsible for ImportAPI calls.</param>
        /// <param name="logger">The logger.</param>
        public RdoImportApiRunner(IRdoImportSettingsBuilder importSettingsBuilder, IImportApiService importApiService, IAPILog logger)
        {
            _importSettingsBuilder = importSettingsBuilder;
            _importApiService = importApiService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RunImportJobAsync(ImportJobContext importJobContext, IntegrationPointInfo integrationPoint)
        {
            _logger.LogInformation("ImportApiRunner for RDO flow started. ImportJobId: {jobId}", importJobContext.JobHistoryGuid);

            RdoImportConfiguration configuration = await CreateConfiguration(integrationPoint).ConfigureAwait(false);

            await _importApiService.CreateImportJobAsync(importJobContext).ConfigureAwait(false);

            await _importApiService.ConfigureRdoImportApiJobAsync(importJobContext, configuration).ConfigureAwait(false);

            await _importApiService.StartImportJobAsync(importJobContext).ConfigureAwait(false);
        }

        private async Task<RdoImportConfiguration> CreateConfiguration(IntegrationPointInfo integrationPoint)
        {
            RdoImportConfiguration configuration = _importSettingsBuilder.Build(integrationPoint.DestinationConfiguration, integrationPoint.FieldMap);

            return configuration;
        }
    }
}
