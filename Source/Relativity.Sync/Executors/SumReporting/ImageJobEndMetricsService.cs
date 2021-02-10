using System;
using System.Threading.Tasks;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class ImageJobEndMetricsService : JobEndMetricsServiceBase, IJobEndMetricsService
	{
		private readonly IJobEndMetricsConfiguration _configuration;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncMetrics _syncMetrics;
		private readonly ISyncLog _logger;

		public ImageJobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration,
			IJobStatisticsContainer jobStatisticsContainer, ISyncMetrics syncMetrics, ISyncLog logger)
			: base(batchRepository, configuration)
		{
			_configuration = configuration;
			_jobStatisticsContainer = jobStatisticsContainer;
			_syncMetrics = syncMetrics;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
		{
			try
			{
				ImageJobEndMetric metric = new ImageJobEndMetric
				{
					JobEndStatus = jobExecutionStatus.GetDescription()
				};

				if (_configuration.JobHistoryToRetryId != null)
				{
					metric.RetryJobEndStatus = jobExecutionStatus.GetDescription();
				}

				await WriteRecordsStatisticsAsync(metric).ConfigureAwait(false);

				await WriteBytesStatistics(metric).ConfigureAwait(false);

				_syncMetrics.Send(metric);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to submit job end metrics.");
			}

			return ExecutionResult.Success();
		}

		private async Task WriteBytesStatistics(ImageJobEndMetric metric)
		{
			ImagesStatistics? imagesStatistics = _jobStatisticsContainer.ImagesStatistics is null
				? (ImagesStatistics?)null
				: await _jobStatisticsContainer.ImagesStatistics.ConfigureAwait(false);

			if (imagesStatistics.HasValue)
			{
				metric.BytesImagesRequested = imagesStatistics.Value.TotalSize;
			}

			// If IAPI job has failed, then it reports 0 bytes transferred and we don't want to send such metric.
			if (_jobStatisticsContainer.FilesBytesTransferred != 0)
			{
				metric.BytesImagesTransferred = _jobStatisticsContainer.FilesBytesTransferred;
			}

			if (_jobStatisticsContainer.TotalBytesTransferred != 0)
			{
				metric.BytesTransferred = _jobStatisticsContainer.TotalBytesTransferred;
			}
		}
	}
}
