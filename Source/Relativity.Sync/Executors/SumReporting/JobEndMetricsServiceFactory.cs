using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.SumReporting
{
    internal class JobEndMetricsServiceFactory : IJobEndMetricsServiceFactory
    {
        private readonly IPipelineSelector _pipelineSelector;

        private readonly IBatchRepository _batchRepository;
        private readonly IJobEndMetricsConfiguration _configuration;
        private readonly IFieldManager _fieldManager;
        private readonly IJobStatisticsContainer _jobStatisticsContainer;
        private readonly ISyncMetrics _syncMetrics;

        private readonly IAPILog _logger;

        public JobEndMetricsServiceFactory(IPipelineSelector pipelineSelector, IBatchRepository batchRepository,
            IJobEndMetricsConfiguration configuration, IFieldManager fieldManager,
            IJobStatisticsContainer jobStatisticsContainer, ISyncMetrics syncMetrics, IAPILog logger)
        {
            _pipelineSelector = pipelineSelector;

            _batchRepository = batchRepository;
            _configuration = configuration;
            _fieldManager = fieldManager;
            _jobStatisticsContainer = jobStatisticsContainer;
            _syncMetrics = syncMetrics;

            _logger = logger;
        }

        public IJobEndMetricsService CreateJobEndMetricsService(bool isSuspended)
        {
            ISyncPipeline syncPipeline = _pipelineSelector.GetPipeline();
            bool isDocumentPipeline = syncPipeline.IsDocumentPipeline();
            bool isImagePipeline = syncPipeline.IsImagePipeline();
            bool isNonDocumentPipeline = syncPipeline.IsNonDocumentPipeline();

            switch (isSuspended)
            {
                case true when isDocumentPipeline:
                    return new DocumentJobSuspendedMetricsService(_syncMetrics);
                case true when isImagePipeline:
                    return new ImageJobSuspendedMetricsService(_syncMetrics);
                case true when isNonDocumentPipeline:
                    return new NonDocumentJobSuspendedMetricsService(_syncMetrics);
                case false when isDocumentPipeline:
                    return new DocumentJobEndMetricsService(_batchRepository, _configuration, _fieldManager, _jobStatisticsContainer, _syncMetrics, _logger);
                case false when isImagePipeline:
                    return new ImageJobEndMetricsService(_batchRepository, _configuration, _jobStatisticsContainer, _syncMetrics, _logger);
                case false when isNonDocumentPipeline:
                    return new NonDocumentJobEndMetricsService(_batchRepository, _configuration, _fieldManager, _jobStatisticsContainer, _syncMetrics, _logger);
                default:
                    _logger.LogWarning(
                        "Unable to determine valid job pipeline type {pipelineType} for metrics send. EmptyJobEndMetricsService is creating...", syncPipeline.GetType());
                    return new EmptyJobEndMetricsService();
            }
        }
    }
}
