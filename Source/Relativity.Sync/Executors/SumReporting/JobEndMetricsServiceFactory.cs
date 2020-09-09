using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using System;

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
			Type type = _pipelineSelector.GetPipeline().GetType();
			if (IsDocumentJob(type))
			{
				return new DocumentJobEndMetricsService(_batchRepository, _configuration, _fieldManager, _jobStatisticsContainer, _syncMetrics, _logger);
			}

			else if (IsImageJob(type))
			{
				return new ImageJobEndMetricsService();
			}
			else
			{
				_logger.LogWarning(
					"Unable to determine valid job pipeline type {pipelineType} for metrics send. EmptyJobEndMetricsService is creating...", type);
				return new EmptyJobEndMetricsService();
			}
		}

		private bool IsDocumentJob(Type pipelineType) => pipelineType == typeof(SyncDocumentRunPipeline) || pipelineType == typeof(SyncDocumentRetryPipeline);

		private bool IsImageJob(Type pipelineType) => pipelineType == typeof(SyncImageRunPipeline) || pipelineType == typeof(SyncImageRetryPipeline);

	}
}
