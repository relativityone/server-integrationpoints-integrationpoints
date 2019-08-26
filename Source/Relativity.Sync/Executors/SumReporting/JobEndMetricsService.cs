using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class JobEndMetricsService : IJobEndMetricsService
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IJobEndMetricsConfiguration _configuration;
		private readonly IFieldManager _fieldManager;
		private readonly ISyncMetrics _syncMetrics;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncLog _logger;

		public JobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration, IFieldManager fieldManager, IJobStatisticsContainer jobStatisticsContainer,
			ISyncMetrics syncMetrics, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_configuration = configuration;
			_fieldManager = fieldManager;
			_jobStatisticsContainer = jobStatisticsContainer;
			_syncMetrics = syncMetrics;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
		{
			try
			{
				string workflowId = _configuration.WorkflowId;
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

				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED, totalTransferred, workflowId);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED, totalFailed, workflowId);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED, totalRequested, workflowId);

				_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, jobExecutionStatus.GetDescription(), workflowId);

				IReadOnlyList<FieldInfoDto> fields = await _fieldManager.GetAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED, fields.Count, workflowId);

				if (_jobStatisticsContainer.TotalBytesTransferred != 0) // if IAPI job has failed, then it reports 0 bytes transferred and we don't want to send such metric.
				{
					_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, _jobStatisticsContainer.TotalBytesTransferred, workflowId);
				}

				long allNativesSize = await _jobStatisticsContainer.NativesBytesRequested.ConfigureAwait(false);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED, allNativesSize, workflowId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to submit job end metrics.");
			}

			return ExecutionResult.Success();
		}
	}
}