using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class DocumentJobEndMetricsService : JobEndMetricsServiceBase, IJobEndMetricsService
	{
		private readonly IFieldManager _fieldManager;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncLog _logger;

		public DocumentJobEndMetricsService(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration, IFieldManager fieldManager, 
			IJobStatisticsContainer jobStatisticsContainer, ISyncMetrics syncMetrics, ISyncLog logger)
			: base(batchRepository, configuration, syncMetrics)
		{
			_fieldManager = fieldManager;
			_jobStatisticsContainer = jobStatisticsContainer;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
		{
			try
			{
				long allNativesSize = await _jobStatisticsContainer.NativesBytesRequested.ConfigureAwait(false);

				await ReportRecordsStatisticsAsync().ConfigureAwait(false);

				ReportJobEndStatus(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_NATIVES_AND_METADATA, jobExecutionStatus);

				await ReportFieldsStatisticsAsync().ConfigureAwait(false);

				ReportBytesStatistics();

				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED, allNativesSize);
				
				ReportLongTextsStatistics();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to submit job end metrics.");
			}

			return ExecutionResult.Success();
		}

		private async Task ReportFieldsStatisticsAsync()
		{
			IReadOnlyList<FieldInfoDto> fields = await _fieldManager.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);
			_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED, fields.Count);
		}

		private void ReportBytesStatistics()
		{
			// If IAPI job has failed, then it reports 0 bytes transferred and we don't want to send such metric.
			if (_jobStatisticsContainer.MetadataBytesTransferred != 0)
			{
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_METADATA_TRANSFERRED, _jobStatisticsContainer.MetadataBytesTransferred);
			}

			if (_jobStatisticsContainer.FilesBytesTransferred != 0)
			{
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_TRANSFERRED, _jobStatisticsContainer.FilesBytesTransferred);
			}

			if (_jobStatisticsContainer.TotalBytesTransferred != 0)
			{
				_syncMetrics.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, _jobStatisticsContainer.TotalBytesTransferred);
			}
		}

		private void ReportLongTextsStatistics()
		{
			int longTextStreamsCount = _jobStatisticsContainer.LongTextStreamsCount;
			long longTextStreamsTotalSizeInBytes = _jobStatisticsContainer.LongTextStreamsTotalSizeInBytes;
			LongTextStreamStatistics largestLongTextStreamStatistics = _jobStatisticsContainer.LargestLongTextStreamStatistics ?? LongTextStreamStatistics.Empty;
			LongTextStreamStatistics smallestLongTextStreamStatistics = _jobStatisticsContainer.SmallestLongTextStreamStatistics ?? LongTextStreamStatistics.Empty;
			long medianLongTextStreamSize = _jobStatisticsContainer.CalculateMedianLongTextStreamSize();

			_logger.LogInformation("Number of long text streams: {longTextStreamsCount} " +
			                       "Total size of all long text streams (MB): {longTextStreamsTotalSize} " +
								   "Largest long text stream size (MB): {largestLongTextSize} " +
								   "Smallest long text stream size (MB): {smallestLongTextSize} " +
								   "Median long text stream size (MB): {medianLongTextStreamSize}",
				longTextStreamsCount, 
				UnitsConverter.BytesToMegabytes(longTextStreamsTotalSizeInBytes),
				UnitsConverter.BytesToMegabytes(largestLongTextStreamStatistics.TotalBytesRead),
				UnitsConverter.BytesToMegabytes(smallestLongTextStreamStatistics.TotalBytesRead),
				UnitsConverter.BytesToMegabytes(medianLongTextStreamSize));

			Tuple<double, double> avgForLessThan1MB = _jobStatisticsContainer.CalculateAverageLongTextStreamSizeAndTime(size => size < UnitsConverter.MegabyteToBytes(1));
			Tuple<double, double> avgBetween1And10MB = _jobStatisticsContainer.CalculateAverageLongTextStreamSizeAndTime(size => size >= UnitsConverter.MegabyteToBytes(1) && size < UnitsConverter.MegabyteToBytes(10));
			Tuple<double, double> avgBetween10And20MB = _jobStatisticsContainer.CalculateAverageLongTextStreamSizeAndTime(streamSize => streamSize >= UnitsConverter.MegabyteToBytes(10) && streamSize < UnitsConverter.MegabyteToBytes(20));
			Tuple<double, double> avgOver20MB = _jobStatisticsContainer.CalculateAverageLongTextStreamSizeAndTime(streamSize => streamSize >= UnitsConverter.MegabyteToBytes(20));

			_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_LESSTHAN1MB, avgForLessThan1MB.Item1);
			_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_LESSTHAN1MB, avgForLessThan1MB.Item2);

			_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWEEN1AND10MB, avgBetween1And10MB.Item1);
			_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWEEN1AND10MB, avgBetween1And10MB.Item2);

			_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWWEEN10AND20MB, avgBetween10And20MB.Item1);
			_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWWEEN10AND20MB, avgBetween10And20MB.Item2);

			_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_OVER20MB, avgOver20MB.Item1);
			_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_OVER20MB, avgOver20MB.Item2);

			List<LongTextStreamStatistics> top10LongTexts = _jobStatisticsContainer
				.LongTextStatistics
				.OrderByDescending(x => x.TotalBytesRead)
				.Take(10)
				.ToList();

			foreach (LongTextStreamStatistics stats in top10LongTexts)
			{
				_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_LARGEST_SIZE, UnitsConverter.BytesToMegabytes(stats.TotalBytesRead));
				_syncMetrics.LogPointInTimeDouble(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_LARGEST_TIME, Math.Round(stats.TotalReadTime.TotalSeconds, 3));
			}
		}
	}
}