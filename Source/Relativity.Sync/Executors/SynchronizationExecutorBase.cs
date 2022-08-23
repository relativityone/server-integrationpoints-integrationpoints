using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.DataExchange;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ADLS;
using Relativity.Sync.Transfer.FileMovementService.Models;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
    internal abstract class SynchronizationExecutorBase<TConfiguration> : IExecutor<TConfiguration> where TConfiguration : ISynchronizationConfiguration
    {
        private readonly BatchRecordType _recordType;
        private readonly IFieldMappings _fieldMappings;
        private readonly IBatchRepository _batchRepository;
        private readonly IJobProgressHandlerFactory _jobProgressHandlerFactory;
        private readonly IJobCleanupConfiguration _jobCleanupConfiguration;
        private readonly IAutomatedWorkflowTriggerConfiguration _automatedWorkflowTriggerConfiguration;
        private readonly Func<IStopwatch> _stopwatchFactory;
        private readonly IUserContextConfiguration _userContextConfiguration;

        protected SynchronizationExecutorBase(
            IImportJobFactory importJobFactory,
            BatchRecordType recordType,
            IBatchRepository batchRepository,
            IJobProgressHandlerFactory jobProgressHandlerFactory,
            IFieldManager fieldManager,
            IFieldMappings fieldMappings,
            IJobStatisticsContainer jobStatisticsContainer,
            IJobCleanupConfiguration jobCleanupConfiguration,
            IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
            Func<IStopwatch> stopwatchFactory,
            ISyncMetrics syncMetrics,
            IUserContextConfiguration userContextConfiguration,
            IAdlsUploader adlsUploader,
            IIsADFTransferEnabled isAdfTransferEnabled,
            IFileLocationManager fileLocationManager,
            IAPILog logger)
        {
            _batchRepository = batchRepository;
            _jobProgressHandlerFactory = jobProgressHandlerFactory;
            _recordType = recordType;
            _fieldMappings = fieldMappings;
            _jobCleanupConfiguration = jobCleanupConfiguration;
            _automatedWorkflowTriggerConfiguration = automatedWorkflowTriggerConfiguration;
            _stopwatchFactory = stopwatchFactory;
            _userContextConfiguration = userContextConfiguration;
            ImportJobFactory = importJobFactory;
            JobStatisticsContainer = jobStatisticsContainer;
            FieldManager = fieldManager;
            SyncMetrics = syncMetrics;
            AdlsUploader = adlsUploader;
            IsAdfTransferEnabled = isAdfTransferEnabled;
            FileLocationManager = fileLocationManager;
            Logger = logger;
        }

        protected ISyncMetrics SyncMetrics { get; }

        protected IJobStatisticsContainer JobStatisticsContainer { get; }

        protected IImportJobFactory ImportJobFactory { get; }

        protected IFieldManager FieldManager { get; }

        protected IAPILog Logger { get; }

        protected IIsADFTransferEnabled IsAdfTransferEnabled { get; }

        protected IAdlsUploader AdlsUploader { get; }

        protected IFileLocationManager FileLocationManager { get; }

        private static ExecutionResult AggregateFailuresOrCancelled(int batchId, params ExecutionResult[] executionResults)
        {
            List<ExecutionResult> failedResults = executionResults.Where(x => x.Status == ExecutionStatus.Failed).ToList();

            if (failedResults.Any())
            {
                string message = $"Processing batch (id: {batchId}) failed: {string.Join(";", failedResults.Select(x => x.Message))}";
                Exception exception = new AggregateException(failedResults.Select(x => x.Exception));
                return ExecutionResult.Failure(message, exception);
            }

            if (executionResults.Any(x => x.Status == ExecutionStatus.Canceled))
            {
                return ExecutionResult.Canceled();
            }

            if (executionResults.Any(x => x.Status == ExecutionStatus.Paused))
            {
                return ExecutionResult.Paused();
            }

            return null;
        }

        private static ExecutionResult AggregateBatchesCompletedWithErrorsResults(Dictionary<int, ExecutionResult> batchesCompletedWithErrorsResults)
        {
            if (batchesCompletedWithErrorsResults.Any())
            {
                string exceptionMessage = string.Join(System.Environment.NewLine, batchesCompletedWithErrorsResults.Select(x => $"BatchID: {x.Key} {x.Value.Message}"));
                AggregateException aggregateException = new AggregateException(
                    exceptionMessage,
                    batchesCompletedWithErrorsResults.Select(x => x.Value.Exception).Where(x => x != null));

                return ExecutionResult.SuccessWithErrors(aggregateException);
            }

            return ExecutionResult.Success();
        }

        public async Task<ExecutionResult> ExecuteAsync(TConfiguration configuration, CompositeCancellationToken token)
        {
            Logger.LogInformation("Creating settings for ImportAPI.");
            UpdateImportSettings(configuration);

            ExecutionResult importAndTagResult = await ExecuteSynchronizationAsync(configuration, token).ConfigureAwait(false);

            _jobCleanupConfiguration.SynchronizationExecutionResult = importAndTagResult;
            _automatedWorkflowTriggerConfiguration.SynchronizationExecutionResult = importAndTagResult;
            return importAndTagResult;
        }

        protected abstract Task<IImportJob> CreateImportJobAsync(TConfiguration configuration, IBatch batch, CancellationToken token);

        protected abstract void UpdateImportSettings(TConfiguration configuration);

        protected abstract void ChildReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer);

        protected abstract Task<TaggingExecutionResult> TagObjectsAsync(IImportJob importJob, ISynchronizationConfiguration configuration, CompositeCancellationToken token);

        protected void ReportBatchMetrics(int batchId, int savedSearchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer)
        {
            SyncMetrics.Send(GetBatchPerformanceMetric(batchId, savedSearchId, batchProcessResult, importApiTimer));
            ChildReportBatchMetrics(batchId, batchProcessResult, batchTime, importApiTimer);
        }

        protected virtual Guid GetExportRunId(TConfiguration configuration)
        {
            return configuration.ExportRunId;
        }

        protected virtual Task<List<FmsBatchInfo>> UploadBatchFilesToAdlsAsync(CompositeCancellationToken token, IImportJob importJob)
        {
            return Task.FromResult(new List<FmsBatchInfo>());
        }

        protected virtual Task<List<CopyListOfFilesResponse>> StartFmsTransfer(List<FmsBatchInfo> fmsBatches, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<CopyListOfFilesResponse>());
        }

        protected int GetDestinationIdentityFieldId()
        {
            FieldMap destinationIdentityField = _fieldMappings.GetFieldMappings().FirstOrDefault(x => x.DestinationField.IsIdentifier);
            if (destinationIdentityField == null)
            {
                const string message = "Cannot find destination identifier field in field mappings.";
                Logger.LogError(message);
                throw new SyncException(message);
            }

            return destinationIdentityField.DestinationField.FieldIdentifier;
        }

        protected string GetSpecialFieldColumnName(IList<FieldInfoDto> specialFields, SpecialFieldType specialFieldType)
        {
            FieldInfoDto specialField = specialFields.FirstOrDefault(x => x.SpecialFieldType == specialFieldType);

            if (specialField == null)
            {
                string message = $"Cannot find special field name: {specialFieldType}";
                Logger.LogError(message);
                throw new SyncException(message);
            }

            return specialField.DestinationFieldName;
        }

        private IMetric GetBatchPerformanceMetric(int batchId, int savedSearchId, BatchProcessResult batchProcessResult, TimeSpan importApiTimer)
        {
            var metric = new BatchEndPerformanceMetric
            {
                Elapsed = (long)importApiTimer.TotalSeconds,
                JobID = batchId,
                WorkspaceID = _jobCleanupConfiguration.SourceWorkspaceArtifactId,
                RecordNumber = batchProcessResult.TotalRecordsTransferred,
                JobSizeGB = UnitsConverter.BytesToGigabytes(batchProcessResult.BytesTransferred),
                JobSizeGB_Metadata = UnitsConverter.BytesToGigabytes(batchProcessResult.MetadataBytesTransferred),
                JobSizeGB_Files = UnitsConverter.BytesToGigabytes(batchProcessResult.FilesBytesTransferred),
                UserID = _userContextConfiguration.ExecutingUserId,
                SavedSearchID = savedSearchId,
                RecordType = _recordType,
                JobStatus = batchProcessResult.ExecutionResult.Status
            };

            return metric;
        }

        private async Task<ExecutionResult> ExecuteSynchronizationAsync(TConfiguration configuration, CompositeCancellationToken token)
        {
            ExecutionResult importAndTagResult;
            try
            {
                Logger.LogInformation("Gathering batches to execute.");

                List<int> batchesIds = (await _batchRepository
                    .GetAllBatchesIdsToExecuteAsync(
                        configuration.SourceWorkspaceArtifactId,
                        configuration.SyncConfigurationArtifactId,
                        GetExportRunId(configuration))
                    .ConfigureAwait(false))
                    .ToList();

                Dictionary<int, ExecutionResult> batchesCompletedWithErrors = new Dictionary<int, ExecutionResult>();

                List<IBatch> executedBatches = (await _batchRepository.GetAllSuccessfullyExecutedBatchesAsync(
                    configuration.SourceWorkspaceArtifactId,
                    configuration.SyncConfigurationArtifactId,
                    GetExportRunId(configuration))
                .ConfigureAwait(false)).ToList();

                using (IJobProgressHandler progressHandler = _jobProgressHandlerFactory.CreateJobProgressHandler(executedBatches))
                {
                    JobStatisticsContainer.RestoreJobStatistics(executedBatches);

                    for (int i = 0; i < batchesIds.Count; i++)
                    {
                        List<FmsBatchInfo> fmsBatches = null;
                        try
                        {
                            int batchId = batchesIds[i];
                            if (token.StopCancellationToken.IsCancellationRequested)
                            {
                                Logger.LogInformation("Import job has been canceled.");
                                return ExecutionResult.Canceled();
                            }

                            await SetImportApiBatchSizeAsync(configuration).ConfigureAwait(false);

                            Logger.LogInformation("Processing batch ID: {batchId} ({index} out of {totalBatches})", batchId, i + 1, batchesIds.Count);

                            IStopwatch batchTimer = GetStartedTimer();
                            IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);

                            using (IImportJob importJob = await CreateImportJobAsync(configuration, batch, token.AnyReasonCancellationToken).ConfigureAwait(false))
                            {
                                using (progressHandler.AttachToImportJob(importJob.SyncImportBulkArtifactJob, batch))
                                {
                                    IStopwatch importApiTimer = GetStartedTimer();
                                    BatchProcessResult batchProcessingResult = await ProcessBatchAsync(importJob, batch, progressHandler, token).ConfigureAwait(false);
                                    importApiTimer.Stop();

                                    fmsBatches = await UploadBatchFilesToAdlsAsync(token, importJob).ConfigureAwait(false);
                                    await StartFmsTransfer(fmsBatches, token.AnyReasonCancellationToken);

                                    TaggingExecutionResult taggingResult = await TagObjectsAsync(importJob, configuration, token).ConfigureAwait(false);
                                    int documentsTaggedCount = taggingResult.TaggedDocumentsCount;
                                    await batch.SetTaggedDocumentsCountAsync(batch.TaggedDocumentsCount + documentsTaggedCount).ConfigureAwait(false);
                                    batchProcessingResult.TotalRecordsTagged = documentsTaggedCount;

                                    if (batchProcessingResult.ExecutionResult.Status == ExecutionStatus.CompletedWithErrors)
                                    {
                                        batchesCompletedWithErrors[batch.ArtifactId] = batchProcessingResult.ExecutionResult;
                                    }

                                    batchTimer.Stop();
                                    ReportBatchMetrics(batchId, configuration.DataSourceArtifactId, batchProcessingResult, batchTimer.Elapsed, importApiTimer.Elapsed);

                                    ExecutionResult failureResult = AggregateFailuresOrCancelled(batch.ArtifactId, batchProcessingResult.ExecutionResult, taggingResult);
                                    if (failureResult != null)
                                    {
                                        return failureResult;
                                    }
                                }
                            }

                            Logger.LogInformation("Batch ID: {batchId} processed successfully ({index} out of {totalBatches})", batch.ArtifactId, i + 1, batchesIds.Count);
                        }
                        finally
                        {
                            FileLocationManager.ClearStoredLocations();
                            if (fmsBatches != null)
                            {
                                IEnumerable<string> batchFilesPaths = fmsBatches.Select(x => x.UploadedBatchFilePath);
                                await CleanUpAdlsBatchFilesAsync(batchFilesPaths, token.AnyReasonCancellationToken).ConfigureAwait(false);
                            }
                        }
                    }

                    importAndTagResult = AggregateBatchesCompletedWithErrorsResults(batchesCompletedWithErrors);
                }
            }
            catch (ImportFailedException ex)
            {
                const string message = "Fatal exception occurred while executing import job.";
                Logger.LogError(ex, message);
                importAndTagResult = ExecutionResult.Failure(message, ex);
            }
            catch (OperationCanceledException oce)
            {
                const string taggingCanceledMessage = "Executing synchronization was interrupted due to the job being canceled.";
                Logger.LogInformation(oce, taggingCanceledMessage);
                importAndTagResult = new ExecutionResult(ExecutionStatus.Canceled, taggingCanceledMessage, oce);
            }
            catch (Exception ex)
            {
                const string message = "Unexpected exception occurred while executing synchronization.";
                Logger.LogError(ex, message);
                importAndTagResult = ExecutionResult.Failure(message, ex);
            }

            return importAndTagResult;
        }

        private async Task<BatchProcessResult> ProcessBatchAsync(IImportJob importJob, IBatch batch, IJobProgressHandler progressHandler, CompositeCancellationToken token)
        {
            BatchProcessResult batchProcessResult = await RunImportJobAsync(importJob, token).ConfigureAwait(false);
            Logger.LogInformation(
                "Batch ID: {batchId} finished processing with status: {status}. Updating batch properties.",
                batch.ArtifactId,
                batchProcessResult.ExecutionResult.Status.ToString());

            int failedDocumentsCount = importJob.SyncImportBulkArtifactJob.ItemStatusMonitor.FailedItemsCount;
            await batch.SetFailedDocumentsCountAsync(batch.FailedDocumentsCount + failedDocumentsCount).ConfigureAwait(false);

            int processedDocumentsCount = importJob.SyncImportBulkArtifactJob.ItemStatusMonitor.ProcessedItemsCount;
            await batch.SetTransferredDocumentsCountAsync(batch.TransferredDocumentsCount + processedDocumentsCount).ConfigureAwait(false);

            int failedItemsCount = progressHandler.GetBatchItemsFailedCount(batch.ArtifactId);
            await batch.SetFailedItemsCountAsync(batch.FailedItemsCount + failedItemsCount).ConfigureAwait(false);

            int processedItemsCount = progressHandler.GetBatchItemsProcessedCount(batch.ArtifactId);
            await batch.SetTransferredItemsCountAsync(batch.TransferredItemsCount + processedItemsCount).ConfigureAwait(false);

            await batch.SetMetadataBytesTransferredAsync(batch.MetadataBytesTransferred + batchProcessResult.MetadataBytesTransferred).ConfigureAwait(false);
            await batch.SetFilesBytesTransferredAsync(batch.FilesBytesTransferred + batchProcessResult.FilesBytesTransferred).ConfigureAwait(false);
            await batch.SetTotalBytesTransferredAsync(batch.TotalBytesTransferred + batchProcessResult.BytesTransferred).ConfigureAwait(false);

            Logger.LogInformation("Batch properties updated: {@batch}", batch);

            if (batchProcessResult.ExecutionResult.Status == ExecutionStatus.Paused)
            {
                batchProcessResult.ExecutionResult = await HandleBatchPausedAsync(batch, failedDocumentsCount, processedDocumentsCount).ConfigureAwait(false);
            }
            else if (batchProcessResult.ExecutionResult.Status == ExecutionStatus.Completed && batch.FailedDocumentsCount > 0)
            {
                // this is going to happen when there were item-level errors in batch before drain-stop, but after resume new IAPI job completed without any new errors
                await SetBatchStatusAsync(batch, ExecutionStatus.CompletedWithErrors);
            }
            else
            {
                await SetBatchStatusAsync(batch, batchProcessResult.ExecutionResult.Status).ConfigureAwait(false);
            }

            batchProcessResult.TotalRecordsRequested = batch.TotalDocumentsCount;
            batchProcessResult.TotalRecordsTransferred = batch.TransferredItemsCount;
            batchProcessResult.TotalRecordsFailed = batch.FailedItemsCount;

            return batchProcessResult;
        }

        private async Task<ExecutionResult> HandleBatchPausedAsync(IBatch batch, int failedDocumentsCount, int transferredDocumentsCount)
        {
            Logger.LogInformation(
                "Batch ID: {batchId} has been paused, determining final batch status. " +
                                   "Failed documents count: {failedDocumentsCount} Transferred documents count: {transferedDocumentsCount}",
                batch.ArtifactId,
                failedDocumentsCount,
                transferredDocumentsCount);

            if (batch.TransferredDocumentsCount == batch.TotalDocumentsCount)
            {
                Logger.LogInformation("All documents in batch have been processed without errors, batch ID: {batchId} is completed.", batch.ArtifactId);
                await SetBatchStatusAsync(batch, ExecutionStatus.Completed).ConfigureAwait(false);
                return ExecutionResult.Success();
            }
            else if (batch.TransferredDocumentsCount + batch.FailedDocumentsCount == batch.TotalDocumentsCount)
            {
                Logger.LogInformation("All documents in batch ID: {batchId} have been processed, but there are item-level errors, setting batch status to Completed With Errors.", batch.ArtifactId);
                await SetBatchStatusAsync(batch, ExecutionStatus.CompletedWithErrors).ConfigureAwait(false);
                return ExecutionResult.SuccessWithErrors();
            }
            else
            {
                int newStartingIndex = batch.StartingIndex + transferredDocumentsCount + failedDocumentsCount;
                Logger.LogInformation("Batch ID: {batchId} has been paused. New starting index: {startingIndex}", batch.ArtifactId, newStartingIndex);
                await batch.SetStartingIndexAsync(newStartingIndex).ConfigureAwait(false);
                await SetBatchStatusAsync(batch, ExecutionStatus.Paused).ConfigureAwait(false);
                return ExecutionResult.Paused();
            }
        }

        private Task SetBatchStatusAsync(IBatch batch, ExecutionStatus executionResultStatus)
        {
            BatchStatus status = executionResultStatus.ToBatchStatus();

            Logger.LogInformation("Setting status {status} for batch {batchId}", status, batch.ArtifactId);
            return batch.SetStatusAsync(status);
        }

        private async Task SetImportApiBatchSizeAsync(TConfiguration configuration)
        {
            int importApiBatchSize = await configuration.GetImportApiBatchSizeAsync().ConfigureAwait(false);
            AppSettings.Instance.ImportBatchSize = importApiBatchSize;
            Logger.LogInformation("Import API batch size set to {importApiBatchSize}", importApiBatchSize);
        }

        private async Task<BatchProcessResult> RunImportJobAsync(IImportJob importJob, CompositeCancellationToken token)
        {
            ImportJobResult importJobResult = await importJob.RunAsync(token).ConfigureAwait(false);

            JobStatisticsContainer.MetadataBytesTransferred += importJobResult.MetadataSizeInBytes;
            JobStatisticsContainer.FilesBytesTransferred += importJobResult.FilesSizeInBytes;
            JobStatisticsContainer.TotalBytesTransferred += importJobResult.JobSizeInBytes;

            return new BatchProcessResult()
            {
                ExecutionResult = importJobResult.ExecutionResult,
                FilesBytesTransferred = importJobResult.FilesSizeInBytes,
                MetadataBytesTransferred = importJobResult.MetadataSizeInBytes,
                BytesTransferred = importJobResult.JobSizeInBytes
            };
        }

        private IStopwatch GetStartedTimer()
        {
            IStopwatch timer = _stopwatchFactory();
            timer.Start();
            return timer;
        }

        private async Task CleanUpAdlsBatchFilesAsync(IEnumerable<string> batchFiles, CancellationToken token)
        {
            if (batchFiles != null)
            {
                List<Task> batchFilesDeleteTasks = new List<Task>();
                foreach (string filePath in batchFiles)
                {
                    batchFilesDeleteTasks.Add(
                        Task.Run(
                            () => { AdlsUploader.DeleteFileAsync(filePath, token); },
                            token));
                }

                await Task.WhenAll(batchFilesDeleteTasks).ConfigureAwait(false);
            }
        }

        protected class BatchProcessResult
        {
            public ExecutionResult ExecutionResult { get; set; }

            public int TotalRecordsRequested { get; set; }

            public int TotalRecordsTransferred { get; set; }

            public int TotalRecordsFailed { get; set; }

            public int TotalRecordsTagged { get; set; }

            public long MetadataBytesTransferred { get; set; }

            public long FilesBytesTransferred { get; set; }

            public long BytesTransferred { get; set; }
        }
    }
}
