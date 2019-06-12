using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class JobEndMetricsService : IJobEndMetricsService
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IJobEndMetricsConfiguration _configuration;
		private readonly ISyncMetrics _syncMetrics;

		public JobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration, ISyncMetrics syncMetrics)
		{
			_batchRepository = batchRepository;
			_configuration = configuration;
			_syncMetrics = syncMetrics;
		}

		public async Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
		{
			int totalTransferred = 0;
			int totalFailed = 0;
			int totalRequested = 0;

			IEnumerable<IBatch> batches = await _batchRepository.GetAllAsync(_configuration.SourceWorkspaceArtifactId, _configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
			foreach (IBatch batch in batches)
			{
				totalTransferred += batch.TransferredItemsCount;
				totalFailed += batch.FailedItemsCount;
				totalRequested += batch.TotalItemsCount;
			}

			_syncMetrics.LogPointInTimeLong(TelemetryConstants.DATA_RECORDS_TRANSFERRED, totalTransferred, _configuration.WorkflowId);
			_syncMetrics.LogPointInTimeLong(TelemetryConstants.DATA_RECORDS_FAILED, totalFailed, _configuration.WorkflowId);
			_syncMetrics.LogPointInTimeLong(TelemetryConstants.DATA_RECORDS_TOTAL_REQUESTED, totalRequested, _configuration.WorkflowId);

			_syncMetrics.LogPointInTimeString(TelemetryConstants.JOB_END_STATUS, jobExecutionStatus.GetDescription(), _configuration.WorkflowId);

			return await Task.FromResult(ExecutionResult.Success()).ConfigureAwait(false);
		}
	}
}