using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

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

				await WriteBytesStatistics(metric).ConfigureAwait(false);

				_syncMetrics.Send(metric);

				ReportLongTextsStatistics();
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

		private async Task WriteBytesStatistics(DocumentJobEndMetric metric)
		{
			long? allNativesSize = _jobStatisticsContainer.NativesBytesRequested is null
				? (long?)null
				: await _jobStatisticsContainer.NativesBytesRequested.ConfigureAwait(false);

			metric.BytesNativesRequested = allNativesSize;

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

			LongTextStreamMetric avgLongTextMetric = new LongTextStreamMetric
			{
				AvgSizeLessThan1MB = avgForLessThan1MB.Item1,
				AvgTimeLessThan1MB = avgForLessThan1MB.Item2,
				AvgSizeLessBetween1and10MB = avgBetween1And10MB.Item1,
				AvgTimeLessBetween1and10MB = avgBetween1And10MB.Item2,
				AvgSizeLessBetween10and20MB = avgBetween10And20MB.Item1,
				AvgTimeLessBetween10and20MB = avgBetween10And20MB.Item2,
				AvgSizeOver20MB = avgOver20MB.Item1,
				AvgTimeOver20MB = avgOver20MB.Item2
			};

			_syncMetrics.Send(avgLongTextMetric);

			List<LongTextStreamStatistics> top10LongTexts = _jobStatisticsContainer
				.LongTextStatistics
				.OrderByDescending(x => x.TotalBytesRead)
				.Take(10)
				.ToList();

			foreach (LongTextStreamStatistics stats in top10LongTexts)
			{
				_syncMetrics.Send(new TopLongTextStreamMetric
				{
					LongTextStreamSize = UnitsConverter.BytesToMegabytes(stats.TotalBytesRead),
					LongTextStreamTime = Math.Round(stats.TotalReadTime.TotalSeconds, 3)
				});
			}
		}
	}
}