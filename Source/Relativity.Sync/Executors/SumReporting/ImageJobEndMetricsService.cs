using System;
using System.Threading.Tasks;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class ImageJobEndMetricsService : JobEndMetricsServiceBase, IJobEndMetricsService
	{
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncLog _logger;

		public ImageJobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration,
			IJobStatisticsContainer jobStatisticsContainer, ISyncMetrics syncMetrics, ISyncLog logger)
			: base(batchRepository, configuration, syncMetrics)
		{
			_jobStatisticsContainer = jobStatisticsContainer;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
		{
			try
			{
				long allImagesSize = await _jobStatisticsContainer.ImagesBytesRequested.ConfigureAwait(false);

				await ReportRecordsStatisticsAsync().ConfigureAwait(false);

				ReportJobEndStatus(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_IMAGES, jobExecutionStatus);

				ReportBytesStatistics();

				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_REQUESTED, allImagesSize);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to submit job end metrics.");
			}

			return ExecutionResult.Success();
		}

		private void ReportBytesStatistics()
		{
			// If IAPI job has failed, then it reports 0 bytes transferred and we don't want to send such metric.
			if (_jobStatisticsContainer.FilesBytesTransferred != 0)
			{
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_TRANSFERRED, _jobStatisticsContainer.FilesBytesTransferred);
			}

			if (_jobStatisticsContainer.TotalBytesTransferred != 0)
			{
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, _jobStatisticsContainer.TotalBytesTransferred);
			}
		}
	}
}
