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
using Relativity.Sync.Transfer.ADLS;
using Relativity.Sync.Transfer.FileMovementService;
using Relativity.Sync.Transfer.FileMovementService.Models;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
    internal class DocumentSynchronizationExecutor : SynchronizationExecutorBase<IDocumentSynchronizationConfiguration>
    {
        private readonly IDocumentTagger _documentTagger;
        private readonly IFmsRunner _fmsRunner;

        public DocumentSynchronizationExecutor(
            IImportJobFactory importJobFactory,
            IBatchRepository batchRepository,
            IJobProgressHandlerFactory jobProgressHandlerFactory,
            IFieldManager fieldManager,
            IFieldMappings fieldMappings,
            IJobStatisticsContainer jobStatisticsContainer,
            IJobCleanupConfiguration jobCleanupConfiguration,
            IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
            Func<IStopwatch> stopwatchFactory,
            ISyncMetrics syncMetrics,
            IDocumentTagger documentTagger,
            IUserContextConfiguration userContextConfiguration,
            IAdlsUploader uploader,
            IIsAdfTransferEnabled isAdfTransferEnabled,
            IFileLocationManager fileLocationManager,
            IFmsRunner fmsRunner,
            IAPILog logger) : base(
            importJobFactory,
            BatchRecordType.Documents,
            batchRepository,
            jobProgressHandlerFactory,
            fieldManager,
            fieldMappings,
            jobStatisticsContainer,
            jobCleanupConfiguration,
            automatedWorkflowTriggerConfiguration,
            stopwatchFactory,
            syncMetrics,
            userContextConfiguration,
            uploader,
            isAdfTransferEnabled,
            fileLocationManager,
            logger)
        {
            _documentTagger = documentTagger;
            _fmsRunner = fmsRunner;
        }

        protected override Task<IImportJob> CreateImportJobAsync(IDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
        {
            return ImportJobFactory.CreateNativeImportJobAsync(configuration, batch, token);
        }

        protected override void UpdateImportSettings(IDocumentSynchronizationConfiguration configuration)
        {
            configuration.IdentityFieldId = GetDestinationIdentityFieldId();

            IList<FieldInfoDto> specialFields = FieldManager.GetNativeSpecialFields().ToList();
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
            int longTextStreamsCount = JobStatisticsContainer.LongTextStreamsCount;
            long longTextStreamsTotalSizeInBytes = JobStatisticsContainer.LongTextStreamsTotalSizeInBytes;
            LongTextStreamStatistics largestLongTextStreamStatistics = JobStatisticsContainer.LargestLongTextStreamStatistics ?? LongTextStreamStatistics.Empty;
            LongTextStreamStatistics smallestLongTextStreamStatistics = JobStatisticsContainer.SmallestLongTextStreamStatistics ?? LongTextStreamStatistics.Empty;
            long medianLongTextStreamSize = JobStatisticsContainer.CalculateMedianLongTextStreamSize();

            Logger.LogInformation(
                "Long-text statistics for batch {batch}: " +
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

            Tuple<double, double> avgForLessThan1MB = JobStatisticsContainer.CalculateAverageLongTextStreamSizeAndTime(size => size < UnitsConverter.MegabyteToBytes(1));
            Tuple<double, double> avgBetween1And10MB = JobStatisticsContainer.CalculateAverageLongTextStreamSizeAndTime(size => size >= UnitsConverter.MegabyteToBytes(1) && size < UnitsConverter.MegabyteToBytes(10));
            Tuple<double, double> avgBetween10And20MB = JobStatisticsContainer.CalculateAverageLongTextStreamSizeAndTime(streamSize => streamSize >= UnitsConverter.MegabyteToBytes(10) && streamSize < UnitsConverter.MegabyteToBytes(20));
            Tuple<double, double> avgOver20MB = JobStatisticsContainer.CalculateAverageLongTextStreamSizeAndTime(streamSize => streamSize >= UnitsConverter.MegabyteToBytes(20));

            List<LongTextStreamStatistics> top10LongTexts = JobStatisticsContainer
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

            SyncMetrics.Send(documentBatchEndMetric);
            JobStatisticsContainer.LongTextStatistics.Clear();
        }

        protected override Task<TaggingExecutionResult> TagObjectsAsync(IImportJob importJob, ISynchronizationConfiguration configuration, CompositeCancellationToken token)
        {
            return _documentTagger.TagObjectsAsync(importJob, configuration, token);
        }

        protected override async Task<List<FmsBatchInfo>> UploadBatchFilesToAdlsAsync(CompositeCancellationToken token, IImportJob importJob)
        {
            if (IsAdfTransferEnabled.Value)
            {
                List<FmsBatchInfo> storedLocations = await GetSuccessfullyPushedDocumentsAsync(importJob).ConfigureAwait(false);
                List<Task> batchesUploadTasks = new List<Task>();
                foreach (FmsBatchInfo storedLocation in storedLocations)
                {
                    if (storedLocation.Files.Count > 0)
                    {
                        batchesUploadTasks.Add(new Task(() =>
                        {
                            string tempBatchFilePath = AdlsUploader.CreateBatchFile(storedLocation, token.AnyReasonCancellationToken);
                            string uploadedBatchFilePath = AdlsUploader.UploadFileAsync(tempBatchFilePath, token.AnyReasonCancellationToken).GetAwaiter().GetResult();
                            storedLocation.UploadedBatchFilePath = uploadedBatchFilePath;
                        }));
                    }
                    else
                    {
                        Logger.LogWarning("Successfully pushed documents not found for FMSBatchInfo.TraceId - {TraceId}", storedLocation.TraceId);
                    }
                }

                await UploadBatchFilesAsync(batchesUploadTasks).ConfigureAwait(false);
                return storedLocations;
            }

            return null;
        }

        protected override async Task PerformFmsTransfer(List<FmsBatchInfo> fmsBatches, CancellationToken cancellationToken)
        {
            if (IsAdfTransferEnabled.Value && fmsBatches.Count > 0)
            {
                List<FmsBatchStatusInfo> statusInfoList = await _fmsRunner.RunAsync(fmsBatches, cancellationToken);
                await _fmsRunner.MonitorAsync(statusInfoList, cancellationToken);
            }
        }

        private async Task<List<FmsBatchInfo>> GetSuccessfullyPushedDocumentsAsync(IImportJob importJob)
        {
            List<FmsBatchInfo> storedLocations = FileLocationManager.GetStoredLocations();
            List<int> successfullyPushedItemsDocumentArtifactIds = (await importJob.GetPushedDocumentArtifactIdsAsync().ConfigureAwait(false)).ToList();
            foreach (FmsBatchInfo storedLocation in storedLocations)
            {
                storedLocation.Files = storedLocation.Files
                    .Where(x =>
                        successfullyPushedItemsDocumentArtifactIds
                            .Contains(x.DocumentArtifactId))
                    .ToList();
            }

            return storedLocations;
        }

        private async Task UploadBatchFilesAsync(List<Task> tasks)
        {
            if (IsAdfTransferEnabled.Value)
            {
                Parallel.ForEach(tasks, t => t.Start());
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }
    }
}
