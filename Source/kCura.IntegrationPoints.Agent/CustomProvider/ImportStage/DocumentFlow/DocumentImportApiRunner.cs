using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow
{
    /// <inheritdoc/>
    internal class DocumentImportApiRunner : IImportApiRunner
    {
        private readonly IDocumentImportSettingsBuilder _importSettingsBuilder;
        private readonly IImportApiService _importApiService;
        private readonly IAPILog _logger;

        /// <summary>
        /// Parameterless constructor for tests purposes only.
        /// </summary>
        public DocumentImportApiRunner()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentImportApiRunner"/> class.
        /// </summary>
        /// <param name="importSettingsBuilder">The builder able to create desired ImportAPI configuration.></param>
        /// <param name="importApiService">The service responsible for ImportAPI calls.</param>
        /// <param name="logger">The logger.</param>
        public DocumentImportApiRunner(IDocumentImportSettingsBuilder importSettingsBuilder, IImportApiService importApiService, IAPILog logger)
        {
            _importSettingsBuilder = importSettingsBuilder;
            _importApiService = importApiService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RunImportJobAsync(ImportJobContext importJobContext, IntegrationPointInfo integrationPointInfo)
        {
            _logger.LogInformation("ImportApiRunner for document flow started. ImportJobId: {jobId}", importJobContext.JobHistoryGuid);

            DocumentImportConfiguration configuration = await _importSettingsBuilder
                .BuildAsync(integrationPointInfo.DestinationConfiguration, integrationPointInfo.FieldMap).ConfigureAwait(false);

            await _importApiService.CreateImportJobAsync(importJobContext).ConfigureAwait(false);

            await _importApiService.ConfigureDocumentImportApiJobAsync(importJobContext, configuration).ConfigureAwait(false);

            await _importApiService.StartImportJobAsync(importJobContext).ConfigureAwait(false);
        }
    }
}
