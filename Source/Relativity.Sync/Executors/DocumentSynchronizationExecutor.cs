using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ADF;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
    internal class DocumentSynchronizationExecutor : SynchronizationExecutorBase<IDocumentSynchronizationConfiguration>
    {
        private readonly IDocumentTagger _documentTagger;
        private readonly IDocumentSynchronizationConfiguration _documentConfiguration;

        public DocumentSynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory, 
			IFieldManager fieldManager, IFieldMappings fieldMappings, IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration,
			IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
			Func<IStopwatch> stopwatchFactory, ISyncMetrics syncMetrics, IDocumentTagger documentTagger, IAPILog logger,
			IUserContextConfiguration userContextConfiguration, IDocumentSynchronizationConfiguration documentConfiguration,
            IADLSUploader uploader, IADFTransferEnabler adfTransferEnabler) : base(importJobFactory, BatchRecordType.Documents, 
            batchRepository, jobProgressHandlerFactory, fieldManager,
			fieldMappings, jobStatisticsContainer, jobCleanupConfiguration, automatedWorkflowTriggerConfiguration, 
            stopwatchFactory, syncMetrics, userContextConfiguration, uploader, adfTransferEnabler, logger)
        {
            _documentTagger = documentTagger;
            _documentConfiguration = documentConfiguration;
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

            if (configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.DoNotImportNativeFiles)
            {
                configuration.FileSizeColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileSize);
                configuration.NativeFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileLocation);
                configuration.FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileFilename);
                configuration.OiFileTypeColumnName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.RelativityNativeType);
                configuration.SupportedByViewerColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.SupportedByViewer);
            }
        }

        protected override void ChildReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer)
        {
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

            List<LongTextStreamStatistics> top10LongTexts = _jobStatisticsContainer
                .LongTextStatistics
                .OrderByDescending(x => x.TotalBytesRead)
                .Take(10)
                .ToList();

            DocumentBatchEndMetric documentBatchEndMetric = new DocumentBatchEndMetric
            {
                TotalRecordsRequested = batchProcessResult.TotalRecordsRequested,
                TotalRecordsTransferred = batchProcessResult.TotalRecordsTransferred,
                TotalRecordsFailed = batchProcessResult.TotalRecordsFailed,
                TotalRecordsTagged = batchProcessResult.TotalRecordsTagged,
                BytesNativesTransferred = batchProcessResult.FilesBytesTransferred,
                BytesMetadataTransferred = batchProcessResult.MetadataBytesTransferred,
                BytesTransferred = batchProcessResult.BytesTransferred,
                BatchImportAPITime = importApiTimer.TotalMilliseconds,
                BatchTotalTime = batchTime.TotalMilliseconds,

                AvgSizeLessThan1MB = avgForLessThan1MB.Item1,
                AvgTimeLessThan1MB = avgForLessThan1MB.Item2,
                AvgSizeLessBetween1and10MB = avgBetween1And10MB.Item1,
                AvgTimeLessBetween1and10MB = avgBetween1And10MB.Item2,
                AvgSizeLessBetween10and20MB = avgBetween10And20MB.Item1,
                AvgTimeLessBetween10and20MB = avgBetween10And20MB.Item2,
                AvgSizeOver20MB = avgOver20MB.Item1,
                AvgTimeOver20MB = avgOver20MB.Item2,

                TopLongTexts = top10LongTexts
            };

            _syncMetrics.Send(documentBatchEndMetric);
            
            _jobStatisticsContainer.LongTextStatistics.Clear();
        }

        protected override Task<TaggingExecutionResult> TagObjectsAsync(IImportJob importJob, ISynchronizationConfiguration configuration, CompositeCancellationToken token)
        {
            return _documentTagger.TagObjectsAsync(importJob, configuration, token);
        }

        protected override async Task UploadLoadFileWithFilePathsToAdlsAsync(CompositeCancellationToken token, IImportJob importJob)
        {
            if (_adfTransferEnabler.IsAdfTransferEnabled && _documentConfiguration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.CopyFiles)
            {
                IEnumerable<int> successfullyPushedItemsDocumentArtifactIds = await importJob.GetPushedDocumentArtifactIdsAsync().ConfigureAwait(false);

                #region Debug

                string destinationLocation = "https://T025.blob.core.windows.net/";
                Dictionary<int, FilePathInfo> locationsDictionary = new Dictionary<int, FilePathInfo>();
                foreach (int pushedItem in successfullyPushedItemsDocumentArtifactIds)
                {
                    Guid guid = Guid.NewGuid();
                    string sourceLocation = "Files\\EDDS1020227\\RV_" + guid;
                    locationsDictionary.Add(pushedItem, new FilePathInfo
                    {
                        ArtifactId = pushedItem,
                        SourceLocationShortToLoadFile = sourceLocation,
                        DestinationLocationFullPathToLink =
                            destinationLocation + sourceLocation.Replace('\\', '/')
                    });
                }

                Guid guid1 = Guid.NewGuid();
                string sourceLocation1 = "Files\\EDDS1020227\\RV_" + guid1;
                locationsDictionary.Add(1, new FilePathInfo
                {
                    ArtifactId = 1,
                    SourceLocationShortToLoadFile = sourceLocation1,
                    DestinationLocationFullPathToLink =
                        destinationLocation + sourceLocation1.Replace('\\', '/')
                });

                #endregion

                locationsDictionary = locationsDictionary.Where(x =>
                        successfullyPushedItemsDocumentArtifactIds.Contains(x.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                string loadFilePath = _adlsUploader.CreateBatchFile(locationsDictionary, token.AnyReasonCancellationToken);
                string adlsLoadFilePath = await _adlsUploader.UploadFileAsync(loadFilePath, token.AnyReasonCancellationToken).ConfigureAwait(false);
            }
        }
    }
}
