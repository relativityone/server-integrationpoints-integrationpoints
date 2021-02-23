﻿using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
	internal class DocumentSynchronizationExecutor : SynchronizationExecutorBase<IDocumentSynchronizationConfiguration>
	{
		public DocumentSynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory, IDocumentTagRepository documentsTagRepository,
			IFieldManager fieldManager, IFieldMappings fieldMappings, IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration, IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
			Func<IStopwatch> stopwatchFactory, ISyncMetrics syncMetrics, ISyncLog logger) : base(importJobFactory, batchRepository, jobProgressHandlerFactory, documentsTagRepository, fieldManager,
			fieldMappings, jobStatisticsContainer, jobCleanupConfiguration, automatedWorkflowTriggerConfiguration, stopwatchFactory, syncMetrics, logger)
		{
		}

		protected override Task<IImportJob> CreateImportJobAsync(IDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			return _importJobFactory.CreateNativeImportJobAsync(configuration, batch, token);
		}

		protected override void UpdateImportSettings(IDocumentSynchronizationConfiguration configuration)
		{
			configuration.IdentityFieldId = GetDestinationIdentityFieldId();

			IList<FieldInfoDto> specialFields = _fieldManager.GetNativeSpecialFields().ToList();
			if (configuration.DestinationFolderStructureBehavior != DestinationFolderStructureBehavior.None)
			{
				configuration.FolderPathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.FolderPath);
			}

			configuration.FileSizeColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileSize);
			configuration.NativeFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileLocation);
			configuration.FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileFilename);
			configuration.OiFileTypeColumnName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.RelativityNativeType);
			configuration.SupportedByViewerColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.SupportedByViewer);
		}

		protected override void ReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer)
		{
			base.ReportBatchMetrics(batchId, batchProcessResult, batchTime, importApiTimer);

			int longTextStreamsCount = _jobStatisticsContainer.LongTextStreamsCount;
			long longTextStreamsTotalSizeInBytes = _jobStatisticsContainer.LongTextStreamsTotalSizeInBytes;
			LongTextStreamStatistics largestLongTextStreamStatistics = _jobStatisticsContainer.LargestLongTextStreamStatistics ?? LongTextStreamStatistics.Empty;
			LongTextStreamStatistics smallestLongTextStreamStatistics = _jobStatisticsContainer.SmallestLongTextStreamStatistics ?? LongTextStreamStatistics.Empty;
			long medianLongTextStreamSize = _jobStatisticsContainer.CalculateMedianLongTextStreamSize();

			_logger.LogInformation("Long-text statistics for batch {batch}: " +
								   "Number of long text streams: {longTextStreamsCount} " +
								   "Total size of all long text streams (MB): {longTextStreamsTotalSize} " +
								   "Largest long text stream size (MB): {largestLongTextSize} " +
								   "Smallest long text stream size (MB): {smallestLongTextSize} " +
								   "Median long text stream size (MB): {medianLongTextStreamSize}",
				batchId,
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

			_jobStatisticsContainer.LongTextStatistics.Clear();

		}
	}
}
