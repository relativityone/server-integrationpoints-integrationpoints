using kCura.IntegrationPoints.Common;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Enumeration;
using Relativity.DataExchange;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    /// <inheritdoc />
    public sealed class ImportApiBuilder : IImportApiBuilder
    {
        private readonly IRelativityTokenProvider _relativityTokenProvider;
        private readonly ILogger<ImportApiBuilder> _logger;

        public ImportApiBuilder(IRelativityTokenProvider relativityTokenProvider, ILogger<ImportApiBuilder> logger)
        {
            _relativityTokenProvider = relativityTokenProvider;
            _logger = logger;
        }

        /// <inheritdoc />
        public IImportAPI CreateImportAPI(string webServiceUrl, int importBatchSize)
        {
            _logger.LogInformation("Attempt to create ImportAPI instance for WebServiceUrl: {{webServiceUrl}}, ImportBatchSize: {importBatchSize}", webServiceUrl, importBatchSize);
            AppSettings.Instance.ImportBatchSize = importBatchSize;
            var importApi = ExtendedImportAPI.CreateByTokenProvider(webServiceUrl, _relativityTokenProvider);
            importApi.ExecutionSource = ExecutionSourceEnum.RIP;
            return importApi;
        }
    }
}
