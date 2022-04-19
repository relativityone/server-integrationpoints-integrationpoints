using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Executors.SumReporting
{
	internal abstract class JobEndMetricsServiceBase
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IJobEndMetricsConfiguration _configuration;

		protected JobEndMetricsServiceBase(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration)
		{
			_batchRepository = batchRepository;
			_configuration = configuration;
		}

		protected void WriteJobDetails<T>(JobEndMetricBase<T> jobEndMetric, ExecutionStatus executionStatus)
			where T : JobEndMetricBase<T>, new()
		{
			jobEndMetric.JobEndStatus = executionStatus.GetDescription();

			if (_configuration.JobHistoryToRetryId != null)
			{
				jobEndMetric.RetryJobEndStatus = executionStatus.GetDescription();
			}

			jobEndMetric.SourceType = _configuration.DataSourceType;
			jobEndMetric.DestinationType = _configuration.DestinationType;
			jobEndMetric.OverwriteMode = _configuration.ImportOverwriteMode;
		}

		protected async Task WriteRecordsStatisticsAsync<T>(JobEndMetricBase<T> jobEndMetric)
			where T: JobEndMetricBase<T>, new()
		{
			jobEndMetric.TotalRecordsTransferred = 0;
			jobEndMetric.TotalRecordsTagged = 0;
			jobEndMetric.TotalRecordsFailed = 0;
			jobEndMetric.TotalRecordsRequested = 0;

			IEnumerable<IBatch> batches = await _batchRepository.GetAllAsync(_configuration.SourceWorkspaceArtifactId, _configuration.SyncConfigurationArtifactId, _configuration.ExportRunId).ConfigureAwait(false);
			foreach (IBatch batch in batches)
			{
				jobEndMetric.TotalRecordsTransferred += batch.TransferredDocumentsCount;
				jobEndMetric.TotalRecordsTagged += batch.TaggedDocumentsCount;
				jobEndMetric.TotalRecordsFailed += batch.FailedDocumentsCount;
				jobEndMetric.TotalRecordsRequested += batch.TotalDocumentsCount;
			}
		}
	}
}
