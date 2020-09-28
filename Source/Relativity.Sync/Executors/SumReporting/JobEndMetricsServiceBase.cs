using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.SumReporting
{
	internal abstract class JobEndMetricsServiceBase
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IJobEndMetricsConfiguration _configuration;
		protected readonly ISyncMetrics _syncMetrics;

		protected JobEndMetricsServiceBase(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration, ISyncMetrics syncMetrics)
		{
			_batchRepository = batchRepository;
			_configuration = configuration;
			_syncMetrics = syncMetrics;
		}

		protected async Task ReportRecordsStatisticsAsync()
		{
			int totalTransferred = 0;
			int totalTagged = 0;
			int totalFailed = 0;
			int totalRequested = 0;

			IEnumerable<IBatch> batches = await _batchRepository.GetAllAsync(_configuration.SourceWorkspaceArtifactId, _configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
			foreach (IBatch batch in batches)
			{
				totalTransferred += batch.TransferredItemsCount;
				totalTagged += batch.TaggedItemsCount;
				totalFailed += batch.FailedItemsCount;
				totalRequested += batch.TotalItemsCount;
			}

			_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED, totalTransferred);
			_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TAGGED, totalTagged);
			_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED, totalFailed);
			_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED, totalRequested);
		}

		protected void ReportJobEndStatus(ExecutionStatus jobExecutionStatus)
		{
			_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, jobExecutionStatus.GetDescription());

			if (_configuration.JobHistoryToRetryId != null)
			{
				_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.RETRY_JOB_END_STATUS, jobExecutionStatus.GetDescription());
			}
		}
	}
}
