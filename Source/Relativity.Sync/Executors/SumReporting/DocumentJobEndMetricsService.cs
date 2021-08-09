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
	internal class DocumentJobEndMetricsService : JobEndMetricsServiceBase, IJobEndMetricsService
	{
		private readonly IJobEndMetricsConfiguration _configuration;
		private readonly IFieldManager _fieldManager;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncMetrics _syncMetrics;
		private readonly ISyncLog _logger;

		public DocumentJobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration, IFieldManager fieldManager, 
			IJobStatisticsContainer jobStatisticsContainer, ISyncMetrics syncMetrics, ISyncLog logger)
			: base(batchRepository, configuration)
		{
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
				DocumentJobEndMetric metric = new DocumentJobEndMetric
				{
					NativeFileCopyMode = _configuration.ImportNativeFileCopyMode
				};

				WriteJobDetails(metric, jobExecutionStatus);

				await WriteRecordsStatisticsAsync(metric).ConfigureAwait(false);

				await WriteFieldsStatisticsAsync(metric).ConfigureAwait(false);

				await WriteBytesStatisticsAsync(metric).ConfigureAwait(false);

				_syncMetrics.Send(metric);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to submit job end metrics.");
			}

			return ExecutionResult.Success();
		}

		private async Task WriteFieldsStatisticsAsync(DocumentJobEndMetric metric)
		{
			IReadOnlyList<FieldInfoDto> fields = await _fieldManager.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);
			metric.TotalMappedFields = fields.Count;
		}

		private async Task WriteBytesStatisticsAsync(DocumentJobEndMetric metric)
		{
			metric.BytesNativesRequested = await GetAllNativesSizeAsync(); ;

			// If IAPI job has failed, then it reports 0 bytes transferred and we don't want to send such metric.
			if (_jobStatisticsContainer.MetadataBytesTransferred != 0)
			{
				metric.BytesMetadataTransferred = _jobStatisticsContainer.MetadataBytesTransferred;
			}

			if (_jobStatisticsContainer.FilesBytesTransferred != 0)
			{
				metric.BytesNativesTransferred = _jobStatisticsContainer.FilesBytesTransferred;
			}

			if (_jobStatisticsContainer.TotalBytesTransferred != 0)
			{
				metric.BytesTransferred = _jobStatisticsContainer.TotalBytesTransferred;
			}
		}

        private async Task<long?> GetAllNativesSizeAsync()
        {
            long? allNativesSize = null;

            try
            {
                allNativesSize = _jobStatisticsContainer.NativesBytesRequested is null
                    ? (long?)null
                    : await _jobStatisticsContainer.NativesBytesRequested.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to get all natives bytes for statistics job");
            }

            return allNativesSize;
        } 

	}
}