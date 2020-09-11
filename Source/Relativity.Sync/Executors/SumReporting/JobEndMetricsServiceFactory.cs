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

		private readonly ISyncLog _logger;

		public JobEndMetricsServiceFactory(IPipelineSelector pipelineSelector, IBatchRepository batchRepository,
			IJobEndMetricsConfiguration configuration, IFieldManager fieldManager,
			IJobStatisticsContainer jobStatisticsContainer, ISyncMetrics syncMetrics, ISyncLog logger)
		{
			_pipelineSelector = pipelineSelector;

			_batchRepository = batchRepository;
			_configuration = configuration;
			_fieldManager = fieldManager;
			_jobStatisticsContainer = jobStatisticsContainer;
			_syncMetrics = syncMetrics;
			
			_logger = logger;
		}	

		public IJobEndMetricsService CreateJobEndMetricsService()
		{
			var syncPipeline = _pipelineSelector.GetPipeline();
			if (syncPipeline.IsDocumentPipeline())
			{
				return new DocumentJobEndMetricsService(_batchRepository, _configuration, _fieldManager, _jobStatisticsContainer, _syncMetrics, _logger);
			}

			else if (syncPipeline.IsImagePipeline())
			{
				return new ImageJobEndMetricsService();
			}
			else
			{
				_logger.LogWarning(
					"Unable to determine valid job pipeline type {pipelineType} for metrics send. EmptyJobEndMetricsService is creating...", syncPipeline.GetType());
				return new EmptyJobEndMetricsService();
			}
		}
	}
}
