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
		private readonly ISyncLog _logger;

		public JobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration, IFieldManager fieldManager, ISyncMetrics syncMetrics, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_configuration = configuration;
			_fieldManager = fieldManager;
			_syncMetrics = syncMetrics;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
		{
			try
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

				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED, totalTransferred, _configuration.WorkflowId);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED, totalFailed, _configuration.WorkflowId);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED, totalRequested, _configuration.WorkflowId);

				_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, jobExecutionStatus.GetDescription(), _configuration.WorkflowId);

				IReadOnlyList<FieldInfoDto> fields = await _fieldManager.GetAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED, fields.Count, _configuration.WorkflowId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to submit job end metrics.");
			}

			return await Task.FromResult(ExecutionResult.Success()).ConfigureAwait(false);
		}
	}
}