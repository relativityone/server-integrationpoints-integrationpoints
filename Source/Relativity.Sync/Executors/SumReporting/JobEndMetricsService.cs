using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncMetrics _syncMetrics;
		private readonly ISyncLog _logger;

		public JobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration, IFieldManager fieldManager, 
			IJobStatisticsContainer jobStatisticsContainer, ISyncMetrics syncMetrics, ISyncLog logger)
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
				ReportLongTextsStatistics();

				int totalTransferred = 0;
				int totalFailed = 0;
				int totalRequested = 0;
				long allNativesSize = await _jobStatisticsContainer.NativesBytesRequested.ConfigureAwait(false);

				IEnumerable<IBatch> batches = await _batchRepository.GetAllAsync(_configuration.SourceWorkspaceArtifactId, _configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
				foreach (IBatch batch in batches)
				{
					totalTransferred += batch.TransferredItemsCount;
					totalFailed += batch.FailedItemsCount;
					totalRequested += batch.TotalItemsCount;
				}

				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED, totalTransferred);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED, totalFailed);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED, totalRequested);

				_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, jobExecutionStatus.GetDescription());

				if(_configuration.JobHistoryToRetryId != null)
				{
					_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.RETRY_JOB_END_STATUS, jobExecutionStatus.GetDescription());
				}

				IReadOnlyList<FieldInfoDto> fields = await _fieldManager.GetAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED, fields.Count);

				if (_jobStatisticsContainer.TotalBytesTransferred != 0) // if IAPI job has failed, then it reports 0 bytes transferred and we don't want to send such metric.
				{
					_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, _jobStatisticsContainer.TotalBytesTransferred);
				}

				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED, allNativesSize);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to submit job end metrics.");
			}

			return ExecutionResult.Success();
		}

		private void ReportLongTextsStatistics()
		{
			int longTextStreamsCount = _jobStatisticsContainer.LongTextStreamsCount;
			long longTextStreamsTotalSizeInBytes = _jobStatisticsContainer.LongTextStreamsTotalSizeInBytes;
			LongTextStreamStatistics largestLongTextStreamStatistics = _jobStatisticsContainer.LargestLongTextStreamStatistics;
			LongTextStreamStatistics smallestLongTextStreamStatistics = _jobStatisticsContainer.SmallestLongTextStreamStatistics;
			long medianLongTextStreamSize = _jobStatisticsContainer.MedianLongTextStreamSizeInBytes;

			_logger.LogInformation("Number of long text streams: {longTextStreamsCount} " +
			                       "Total size of all long text streams (bytes): {longTextStreamsTotalSize} " +
			                       "Largest long text stream size (bytes): {largestLongTextSize} " +
			                       "Smallest long text stream size (bytes): {smallestLongTextSize} " +
			                       "Average long text stream size (bytes): {averageLongTextSize}",
				longTextStreamsCount, longTextStreamsTotalSizeInBytes, largestLongTextStreamStatistics, smallestLongTextStreamStatistics, medianLongTextStreamSize);

			Tuple<double, double> avgForLessThan1MB = CalculateAverageSizeAndTime(size => size < MegabyteToBytes(1));
			Tuple<double, double> avgBetween1And10MB = CalculateAverageSizeAndTime(size => size >= MegabyteToBytes(1) && size < MegabyteToBytes(10));
			Tuple<double, double> avgBetween10And20MB = CalculateAverageSizeAndTime(streamSize => streamSize >= MegabyteToBytes(10) && streamSize < MegabyteToBytes(20));
			Tuple<double, double> avgOver20MB = CalculateAverageSizeAndTime(streamSize => streamSize >= MegabyteToBytes(20));

			_syncMetrics.LogPointInTimeDouble("AverageSize.LessThan1MB", avgForLessThan1MB.Item1);
			_syncMetrics.LogPointInTimeDouble("AverageTime.LessThan1MB", avgForLessThan1MB.Item2);

			_syncMetrics.LogPointInTimeDouble("AverageSize.Between1And10MB", avgBetween1And10MB.Item1);
			_syncMetrics.LogPointInTimeDouble("AverageTime.Between1And10MB", avgBetween1And10MB.Item2);

			_syncMetrics.LogPointInTimeDouble("AverageSize.Between10And20MB", avgBetween10And20MB.Item1);
			_syncMetrics.LogPointInTimeDouble("AverageTime.Between10And20MB", avgBetween10And20MB.Item2);

			_syncMetrics.LogPointInTimeDouble("AverageSize.Over20MB", avgOver20MB.Item1);
			_syncMetrics.LogPointInTimeDouble("AverageTime.Over20MB", avgOver20MB.Item2);

			List<LongTextStreamStatistics> top10LongTexts = _jobStatisticsContainer
				.LongTextStatistics
				.OrderByDescending(x => x.TotalBytesRead)
				.Take(10)
				.ToList();

			foreach (LongTextStreamStatistics stats in top10LongTexts)
			{
				_syncMetrics.LogPointInTimeDouble("LargestLongText.Size", BytesToMegabytes(stats.TotalBytesRead));
				_syncMetrics.LogPointInTimeDouble("LargestLongText.Time", Math.Round(stats.TotalReadTime.TotalSeconds, 3));
			}
		}

		private Tuple<double, double> CalculateAverageSizeAndTime(Func<long, bool> streamSizePredicate)
		{
			List<Tuple<double, double>> sizeAndTimeTuples = _jobStatisticsContainer
				.LongTextStatistics
				.Where(x => streamSizePredicate(x.TotalBytesRead))
				.Select(x => new Tuple<double, double>(
					BytesToMegabytes(x.TotalBytesRead), 
					x.TotalReadTime.TotalSeconds))
				.ToList();

			double averageSizeInMB = sizeAndTimeTuples.Select(x => x.Item1).Average();
			double averageTimeInSeconds = sizeAndTimeTuples.Select(x => x.Item2).Average();
			return new Tuple<double, double>(averageSizeInMB, averageTimeInSeconds);
		}

		private long MegabyteToBytes(long megabytes)
		{
			return megabytes * 1024 * 1024;
		}

		private double BytesToMegabytes(long bytes)
		{
			return bytes / 1024.0 / 1024.0;
		}

	}
}