using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class NonDocumentJobEndMetricsService : JobEndMetricsServiceBase, IJobEndMetricsService
	{
        private readonly IFieldManager _fieldManager;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncMetrics _syncMetrics;
		private readonly ISyncLog _logger;

		public NonDocumentJobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration, IFieldManager fieldManager, 
			IJobStatisticsContainer jobStatisticsContainer, ISyncMetrics syncMetrics, ISyncLog logger)
			: base(batchRepository, configuration)
		{
            _fieldManager = fieldManager;
			_jobStatisticsContainer = jobStatisticsContainer;
			_syncMetrics = syncMetrics;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
		{
			try
			{
				NonDocumentJobEndMetric metric = new NonDocumentJobEndMetric();

				WriteJobDetails(metric, jobExecutionStatus);

				await WriteRecordsStatisticsAsync(metric).ConfigureAwait(false);

				await WriteFieldsStatisticsAsync(metric).ConfigureAwait(false);

                WriteBytesStatistics(metric);

				_syncMetrics.Send(metric);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to submit job end metrics.");
			}

			return ExecutionResult.Success();
		}

		private async Task WriteFieldsStatisticsAsync(NonDocumentJobEndMetric metric)
		{
            IReadOnlyList<FieldInfoDto> fields = await _fieldManager.GetMappedFieldsNonDocumentWithoutLinksAsync(CancellationToken.None).ConfigureAwait(false);
            metric.TotalMappedFields = fields.Count;
            metric.TotalAvailableFields = _fieldManager.GetAllAvailableFieldsToMap().Count;
        }

		private void WriteBytesStatistics(NonDocumentJobEndMetric metric)
		{
            // If IAPI job has failed, then it reports 0 bytes transferred and we don't want to send such metric.
			if (_jobStatisticsContainer.MetadataBytesTransferred != 0)
			{
				metric.BytesMetadataTransferred = _jobStatisticsContainer.MetadataBytesTransferred;
			}

			if (_jobStatisticsContainer.TotalBytesTransferred != 0)
			{
				metric.BytesTransferred = _jobStatisticsContainer.TotalBytesTransferred;
			}
		}
    }
}