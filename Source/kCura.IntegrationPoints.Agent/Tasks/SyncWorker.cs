using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Authentication;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class SyncWorker : IntegrationPointTaskBase, ITaskWithJobHistory
    {
        private readonly IProviderTypeService _providerTypeService;
        private readonly IAPILog _logger;
        private readonly IJobStatisticsService _statisticsService;

        private IEnumerable<IBatchStatus> _batchStatus;

        public SyncWorker(
            ICaseServiceContext caseServiceContext,
            IHelper helper,
            IDataProviderFactory dataProviderFactory,
            ISerializer serializer,
            ISynchronizerFactory appDomainRdoSynchronizerFactory,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IJobManager jobManager,
            IEnumerable<IBatchStatus> statuses,
            IJobStatisticsService statisticsService,
            IManagerFactory managerFactory,
            IJobService jobService,
            IProviderTypeService providerTypeService,
            IIntegrationPointService integrationPointService,
            IDiagnosticLog diagnosticLog)
            : base(
                caseServiceContext,
                helper,
                dataProviderFactory,
                serializer,
                appDomainRdoSynchronizerFactory,
                jobHistoryService,
                jobHistoryErrorService,
                jobManager,
                managerFactory,
                jobService,
                integrationPointService,
                diagnosticLog)
        {
            BatchStatus = statuses;
            _providerTypeService = providerTypeService;
            _statisticsService = statisticsService;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<SyncWorker>();
        }

        public IEnumerable<IBatchStatus> BatchStatus
        {
            get { return _batchStatus ?? (_batchStatus = new List<IBatchStatus>()); }
            set { _batchStatus = value; }
        }

        protected IJobStopManager JobStopManager { get; private set; }

        public void Execute(Job job)
        {
            LogExecuteStart(job);

            foreach (IBatchStatus batchComplete in BatchStatus)
            {
                batchComplete.OnJobStart(job);
            }

            ExecuteTask(job);

            LogExecuteEnd(job);
        }

        protected virtual void ExecuteTask(Job job)
        {
            try
            {
                LogExecuteTaskStart(job);

                SetIntegrationPoint(job);

                DeserializeAndSetupIntegrationPointsConfigurationForStatisticsService(IntegrationPointDto);

                List<string> entryIDs = GetEntryIDs(job);

                SetJobHistory();

                ExtendSourceConfigurationWithBatchStartingIndex(job);

                ConfigureJobStopManager(job, true);

                if (IntegrationPointDto.SourceProvider == 0)
                {
                    LogUnknownSourceProvider(job);
                    throw new ArgumentException("Cannot import source provider with unknown id.");
                }
                if (IntegrationPointDto.DestinationProvider == 0)
                {
                    LogUnknownDestinationProvider(job);
                    throw new ArgumentException("Cannot import destination provider with unknown id.");
                }

                JobStopManager?.ThrowIfStopRequested();

                ExecuteImport(
                    IntegrationPointDto.FieldMappings,
                    new DataSourceProviderConfiguration(IntegrationPointDto.SourceConfiguration, IntegrationPointDto.SecuredConfiguration),
                    IntegrationPointDto.DestinationConfiguration,
                    entryIDs,
                    SourceProvider,
                    DestinationProvider,
                    job);

                LogExecuteTaskSuccesfulEnd(job);
            }
            catch (OperationCanceledException e)
            {
                LogJobStoppedException(job, e);

                // the job has been stopped.
            }
            catch (AuthenticationException e)
            {
                LogAuthenticationException(job, e);
                JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, e.Message, e.StackTrace);
            }
            catch (Exception ex)
            {
                LogExecutingTaskError(job, ex);
                JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
                if (ex is IntegrationPointsException)
                {
                    throw;
                }
            }
            finally
            {
                PostExecute(job);
                LogExecuteTaskFinalize(job);
            }
        }

        protected void ConfigureJobStopManager(Job job, bool supportsDrainStop)
        {
            JobStopManager = ManagerFactory.CreateJobStopManager(JobService, JobHistoryService, BatchInstance, job.JobId, supportsDrainStop: supportsDrainStop, DiagnosticLog);
            JobHistoryErrorService.JobStopManager = JobStopManager;
        }

        protected virtual List<string> GetEntryIDs(Job job)
        {
            TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
            BatchInstance = taskParameters.BatchInstance;
            if (taskParameters.BatchParameters != null)
            {
                if (taskParameters.BatchParameters is JArray)
                {
                    return ((JArray)taskParameters.BatchParameters).ToObject<List<string>>();
                }
                if (taskParameters.BatchParameters is List<string>)
                {
                    return (List<string>)taskParameters.BatchParameters;
                }
            }
            return new List<string>();
        }

        protected void PostExecute(Job job)
        {
            try
            {
                job = JobService.GetJob(job.JobId);

                LogPostExecuteStart(job);
                JobHistoryErrorService.CommitErrors();
                UpdateJobHistoryStopState(job);

                // if there is no StopManager, batch should finish
                bool isBatchFinished = (!JobStopManager?.ShouldDrainStop) ?? true;
                bool isJobComplete = JobManager.CheckBatchOnJobComplete(job, BatchInstance.ToString(), isBatchFinished);

                if (isJobComplete)
                {
                    OnJobComplete(job);
                }
            }
            catch (Exception e)
            {
                LogPostExecuteError(job, e);
                JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);

                // we want to rethrow, so it can be added to error tab if necessary
                if (e is IntegrationPointsException)
                {
                    throw;
                }
            }
            finally
            {
                JobHistoryErrorService.CommitErrors();
                LogPostExecuteFinalize(job);
                JobStopManager?.Dispose();
            }
        }

        protected virtual void ExecuteImport(
            IEnumerable<FieldMap> fieldMap,
            DataSourceProviderConfiguration configuration,
            string destinationConfiguration,
            List<string> entryIDs,
            SourceProvider sourceProviderRdo,
            DestinationProvider destinationProvider,
            Job job)
        {
            LogExecuteImportStart(job);

            FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();

            JobStopManager?.ThrowIfStopRequested();

            IDataSourceProvider sourceProvider = GetSourceProvider(SourceProvider, job);

            JobStopManager?.ThrowIfStopRequested();

            IDataSynchronizer dataSynchronizer = GetDestinationProvider(destinationProvider, destinationConfiguration, job);

            // Obtain settings for destination configuration
            ImportSettings destinationSettings = Serializer.Deserialize<ImportSettings>(destinationConfiguration);

            // Make non-Relativity providers log the document RDOs' created by/modified by field as the user who submitted the job.
            // (Adjustment only needs to be made before IDataSynchronizer.SyncData)
            if (dataSynchronizer is RdoSynchronizer)
            {
                destinationSettings.OnBehalfOfUserId = job.SubmittedBy;
                destinationSettings.CorrelationId = BatchInstance;
                destinationSettings.JobID = job.RootJobId;
                destinationSettings.Provider = IntegrationPointDto.GetProviderType(_providerTypeService).ToString();
                destinationConfiguration = Serializer.Serialize(destinationSettings);
            }

            // Extract source fields from field map
            List<FieldEntry> sourceFields = GetSourceFields(fieldMaps);
            if (JobStopManager?.ShouldDrainStop != true)
            {
                _logger.LogInformation("Start reading data from {sourceProvider}...", sourceProvider?.GetType());
                using (IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, configuration))
                {
                    _logger.LogInformation("SourceDataReader was created for {entryIDsCount}.", entryIDs.Count);
                    SetupSubscriptions(dataSynchronizer, job);

                    _logger.LogInformation("Reading source data");
                    IEnumerable<IDictionary<FieldEntry, object>> sourceData = GetSourceData(sourceFields, sourceDataReader);
                    JobStopManager?.ThrowIfStopRequested();

                    _logger.LogInformation("Start SyncData...");
                    dataSynchronizer.SyncData(sourceData, fieldMaps, destinationConfiguration, JobStopManager, DiagnosticLog);
                    _logger.LogInformation("SyncData Completed. Processed rows: {processedRows}", dataSynchronizer.TotalRowsProcessed);
                }
            }
            else
            {
                _logger.LogInformation("Skipping synchronizer setup because DrainStop was requested");
            }

            Guid batchInstance = Guid.Parse(JobHistory.BatchInstance);
            JobHistory = JobHistoryService.GetRdo(batchInstance);
            int totalRowsCountInBatch = GetRowsCountForBatch(job.JobDetails);

            bool shouldDrainStopBatch = ShouldDrainStopBatch(dataSynchronizer.TotalRowsProcessed, totalRowsCountInBatch);

            if (shouldDrainStopBatch)
            {
                job.JobDetails = SkipProcessedItems(job.JobDetails, dataSynchronizer.TotalRowsProcessed);
                JobService.UpdateJobDetails(job);

                job.StopState = StopState.DrainStopped;
                JobService.UpdateStopState(new List<long> { job.JobId }, job.StopState);
                _logger.LogInformation("Job {jobId} was Drain-stopped on SyncWorker level.", job.JobId);
            }

            _logger.LogInformation("Stop checking Drain-Stop.");
            JobStopManager?.StopCheckingDrainStopAndUpdateStopState(job, shouldDrainStopBatch);

            LogExecuteImportSuccesfulEnd(job);
        }

        protected void SetupJobHistoryErrorSubscriptions(IDataSynchronizer synchronizer)
        {
            JobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
        }

        private bool ShouldDrainStopBatch(int processedItemCount, int totalRowsCountInBatch)
        {
            bool notAllItemsProcessed = processedItemCount < totalRowsCountInBatch;
            bool drainStopRequested = JobStopManager?.ShouldDrainStop == true;

            bool shouldBeDrainStopped = drainStopRequested && notAllItemsProcessed;

            _logger.LogInformation(
                "Checking if batch should be Drain-Stopped - {shouldBeDrainStopped}: " +
                    "Processed Rows Count - {processedItemsCount}, " +
                    "Total Rows Count - {totalRowsCount} " +
                    "Drain-Stop Requested - {drainStopRequested}",
                shouldBeDrainStopped,
                processedItemCount,
                totalRowsCountInBatch,
                drainStopRequested);

            return shouldBeDrainStopped;
        }

        private void MarkJobAsDrainStopped()
        {
            JobHistory.JobStatus = new ChoiceRef(new List<Guid> { JobStatusChoices.JobHistorySuspendedGuid });
            JobHistoryService.UpdateRdoWithoutDocuments(JobHistory);
            _logger.LogInformation("Marking job history with id = {id} as Suspended", JobHistory.ArtifactId);
        }

        private int GetRowsCountForBatch(string jobDetails)
        {
            TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(jobDetails);
            return GetRecordsIds(taskParameters).Count;
        }

        private string SkipProcessedItems(string jobDetails, int processedItemCount)
        {
            TaskParameters parameters = Serializer.Deserialize<TaskParameters>(jobDetails);

            List<string> list = GetRecordsIds(parameters)
                .Skip(processedItemCount).ToList();

            _logger.LogInformation("IDs Count {count} left to be processed after Drain-Stop.", list.Count);

            parameters.BatchParameters = list;

            return Serializer.Serialize(parameters);
        }

        private void ExtendSourceConfigurationWithBatchStartingIndex(Job job)
        {
            try
            {
                _logger.LogInformation(
                    "ExtendSourceConfigurationWithBatchStartingIndex - execution start jobId: {jobId}, jobDetails: {jobDetails}",
                    job?.JobId,
                    job?.JobDetails);

                TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
                if (taskParameters.BatchStartingIndex != null)
                {
                    _logger.LogInformation(
                        "ExtendSourceConfigurationWithBatchStartingIndex - attempt to add batchStartingIndex: {batchStartingIndex} to sourceConfiguration: {sourceConfiguration}",
                        taskParameters?.BatchStartingIndex, IntegrationPointDto?.SourceConfiguration);

                    Dictionary<string, object> sourceConfiguration = Serializer.Deserialize<Dictionary<string, object>>(IntegrationPointDto?.SourceConfiguration);

                    _logger.LogInformation(
                        "ExtendSourceConfigurationWithBatchStartingIndex - sourceConfiguration dictionary size: {sourceConfigurationSize}",
                        sourceConfiguration?.Count);

                    sourceConfiguration.Add(nameof(TaskParameters.BatchStartingIndex), taskParameters.BatchStartingIndex);
                    IntegrationPointDto.SourceConfiguration = Serializer.Serialize(sourceConfiguration);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExtendSourceConfigurationWithBatchStartingIndex - execution failed");
            }
        }

        private List<string> GetRecordsIds(TaskParameters taskParameters)
        {
            return Serializer.Deserialize<List<string>>(taskParameters.BatchParameters.ToString());
        }

        private void OnJobComplete(Job job)
        {
            try
            {
                if (JobStopManager?.IsStopRequested() == true)
                {
                    IList<Job> jobs = JobManager.GetJobsByBatchInstanceId(IntegrationPointDto.ArtifactId, BatchInstance);
                    if (jobs.Any())
                    {
                        List<long> ids = jobs.Select(agentJob => agentJob.JobId).ToList();

                        LogUpdateStopStateToUnstoppable(ids);
                        JobService.UpdateStopState(
                            jobs.Select(agentJob => agentJob.JobId).ToList(),
                            StopState.Unstoppable);
                    }
                }
            }
            catch (Exception e)
            {
                LogStatusUpdateError(job, e);

                // IGNORE ERROR. It is possible that the user stop the job in between disposing job history manager and the updating the stop state.
            }

            SetErrorStatusesToExpiredIfStopped(job);

            foreach (IBatchStatus completedItem in BatchStatus)
            {
                try
                {
                    completedItem.OnJobComplete(job);
                }
                catch (Exception e)
                {
                    LogCompletingJobError(job, e, completedItem);
                    JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);

                    // we want to rethrow, so it can be added to error tab if necessary
                    if (e is IntegrationPointsException)
                    {
                        throw;
                    }
                }
            }

            List<long> jobsToUpdate =
                JobManager.GetJobsByBatchInstanceId(IntegrationPointDto.ArtifactId, BatchInstance)
                    .Where(x => x.StopState != StopState.DrainStopped && x.StopState != StopState.DrainStopping)
                    .Select(agentJob => agentJob.JobId)
                    .ToList();

            if (jobsToUpdate.Any())
            {
                LogUpdateStopStateToNone(jobsToUpdate);
                JobService.UpdateStopState(jobsToUpdate, StopState.None);
            }
        }

        private void UpdateJobHistoryStopState(Job job)
        {
            BatchStatusQueryResult batchesStatuses = JobManager.GetBatchesStatuses(job, BatchInstance.ToString());

            bool otherBatchesProcessing = batchesStatuses.ProcessingCount > 1; // one is the current batch, so if there are other batches it means at least 2
            bool atLeastOneSuspended = batchesStatuses.SuspendedCount > 0;

            if (!otherBatchesProcessing)
            {
                if (atLeastOneSuspended || (job.StopState == StopState.DrainStopped || job.StopState == StopState.DrainStopping))
                {
                    MarkJobAsDrainStopped();
                }
            }
        }

        private void SetupStatisticsSubscriptions(IDataSynchronizer synchronizer, Job job)
        {
            _statisticsService.Subscribe(synchronizer as IBatchReporter, job);
        }

        private void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
        {
            _logger.LogInformation("Setup SyncWorker subscriptions for Job {jobId}", job?.JobId);
            SetupStatisticsSubscriptions(synchronizer, job);
            SetupJobHistoryErrorSubscriptions(synchronizer);
        }

        private void SetErrorStatusesToExpiredIfStopped(Job job)
        {
            try
            {
                if (JobStopManager?.IsStopRequested() == true)
                {
                    IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager();
                    jobHistoryManager.SetErrorStatusesToExpired(CaseServiceContext.WorkspaceID, JobHistory.ArtifactId);
                }
            }
            catch (Exception e)
            {
                LogUpdatingStoppedJobStatusError(job, e);

                // Ignore error. Job history error status only set for the consistency. This will not affect re-running the job.
            }
        }

        private void DeserializeAndSetupIntegrationPointsConfigurationForStatisticsService(IntegrationPointDto ip)
        {
            SourceConfiguration sourceConfiguration = null;
            ImportSettings importSettings = null;
            try
            {
                sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(ip?.SourceConfiguration);
                importSettings = Serializer.Deserialize<ImportSettings>(ip?.DestinationConfiguration);
            }
            catch (Exception ex)
            {
                LogDeserializeIntegrationPointsConfigurationForStatisticsServiceWarning(ex);
            }

            SetupIntegrationPointsConfigurationForStatisticsService(sourceConfiguration, importSettings);
        }

        private void SetupIntegrationPointsConfigurationForStatisticsService(SourceConfiguration sourceConfiguration, ImportSettings importSettings)
        {
            if (sourceConfiguration == null || importSettings == null)
            {
                LogSkippingSetupIntegrationPointsConfigurationForStatisticsServiceWarning();
            }

            try
            {
                _statisticsService.SetIntegrationPointConfiguration(importSettings, sourceConfiguration);
            }
            catch (Exception ex)
            {
                LogSetupIntegrationPointsConfigurationForStatisticsServiceError(sourceConfiguration, importSettings, ex);
                throw;
            }
        }

        #region Logging
        private void LogSkippingSetupIntegrationPointsConfigurationForStatisticsServiceWarning()
        {
            _logger.LogWarning("Skipping setup of Integration Point configuration for statistics service.");
        }

        private void LogSetupIntegrationPointsConfigurationForStatisticsServiceError(SourceConfiguration sourceConfiguration, ImportSettings importSettings, Exception ex)
        {
            var settingsForLogging = new ImportSettingsForLogging(importSettings);
            var sourceConfigForLogging = new SourceConfigurationForLogging(sourceConfiguration);

            string msg =
                "Failed to set up integration point configuration for statistics service. SourceConfiguration: {sourceConfiguration}. ImportSettings: {importSettings}";
            _logger.LogError(ex, msg, sourceConfigForLogging, settingsForLogging);
        }

        private void LogDeserializeIntegrationPointsConfigurationForStatisticsServiceWarning(Exception ex)
        {
            string msg = "Failed to deserialize integration point configuration for statistics service.";
            _logger.LogWarning(ex, msg);
        }

        private void LogExecutingTaskError(Job job, Exception ex)
        {
            _logger.LogError(ex, "Failed to execute SyncWorker task for Job ID {JobId}.", job.JobId);
        }

        private void LogAuthenticationException(Job job, AuthenticationException e)
        {
            _logger.LogError(e, "Error occurred during authentication for Job ID {JobId}.", job.JobId);
        }

        private void LogJobStoppedException(Job job, OperationCanceledException e)
        {
            _logger.LogInformation(e, "Job {JobId} has been stopped.", job.JobId);
        }

        private void LogUnknownDestinationProvider(Job job)
        {
            _logger.LogError("Destination provider for Job ID {JobId} is unknown.", job.JobId);
        }

        private void LogUnknownSourceProvider(Job job)
        {
            _logger.LogError("Source provider for Job ID {JobId} is unknown.", job.JobId);
        }

        private void LogPostExecuteError(Job job, Exception e)
        {
            _logger.LogError(e, "Failed to execute PostExecute for job {JobId}.", job.JobId);
        }

        private void LogCompletingJobError(Job job, Exception exception, IBatchStatus batchStatus)
        {
            _logger.LogError(exception, "Failed to complete job {JobId}. Error occured in BatchStatus {BatchStatusType}.", job.JobId, batchStatus.GetType());
        }

        private void LogStatusUpdateError(Job job, Exception e)
        {
            _logger.LogError(e, "Error occurred during updating job {JobId} status in PostExecute.", job.JobId);
        }

        private void LogUpdatingStoppedJobStatusError(Job job, Exception exception)
        {
            _logger.LogError(exception, "Failed to update job ({JobId}) status after job has been stopped.", job.JobId);
        }

        private void LogExecuteEnd(Job job)
        {
            _logger.LogInformation("Finished execution of job in SyncWorker for Job ID: {JobId}", job.JobId);
        }

        private void LogExecuteStart(Job job)
        {
            _logger.LogInformation("Starting execution of job in SyncWorker for Job ID: {JobId}", job.JobId);
        }

        private void LogExecuteImportSuccesfulEnd(Job job)
        {
            _logger.LogInformation("Succesfully finished execution of import in SyncWorker for Job ID: {JobId}.", job.JobId);
        }

        private void LogExecuteImportStart(Job job)
        {
            _logger.LogInformation("Starting execution of import in SyncWorker for Job ID: {JobId}.", job.JobId);
        }

        private void LogExecuteTaskFinalize(Job job)
        {
            _logger.LogInformation("Finalized execution of task in SyncWorker for Job ID: {JobId}", job.JobId);
        }

        private void LogExecuteTaskSuccesfulEnd(Job job)
        {
            _logger.LogInformation("Succesfully finished execution of task in SyncWorker for Job ID: {JobId}.", job.JobId);
        }

        private void LogExecuteTaskStart(Job job)
        {
            _logger.LogInformation("Starting execution of task in SyncWorker for Job ID: {JobId}", job.JobId);
        }

        private void LogPostExecuteFinalize(Job job)
        {
            _logger.LogInformation("Finalized post execute method in SyncWorker for Job ID: {JobId}.", job.JobId);
        }

        private void LogUpdateStopStateToNone(List<long> ids)
        {
            _logger.LogInformation("Updating stop state to None in SyncWorker Job ID: {ids}.", ids);
        }

        private void LogUpdateStopStateToUnstoppable(List<long> ids)
        {
            _logger.LogInformation("Updating stop state to Unstoppable in SyncWorker Job ID: {ids}.", ids);
        }

        private void LogPostExecuteStart(Job job)
        {
            _logger.LogInformation("Starting post execute method in SyncWorker for Job ID: {JobId}.", job.JobId);
        }
        #endregion
    }
}
