using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common.RelativitySync;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public class IntegrationPointService : IntegrationPointServiceBase, IIntegrationPointService
    {
        private const string _VALIDATION_FAILED = "Failed to submit integration job. Integration Point validation failed.";

        private readonly IAPILog _logger;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobManager _jobManager;
        private readonly IMessageService _messageService;
        private readonly IProviderTypeService _providerTypeService;
        private readonly IIntegrationPointRepository _integrationPointRepository;
        private readonly ITaskParametersBuilder _taskParametersBuilder;
        private readonly IRelativitySyncConstrainsChecker _relativitySyncConstrainsChecker;
        private readonly IRelativitySyncAppIntegration _relativitySyncAppIntegration;

        public IntegrationPointService(
            IHelper helper,
            ICaseServiceContext context,
            IIntegrationPointSerializer serializer,
            IChoiceQuery choiceQuery,
            IJobManager jobManager,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IManagerFactory managerFactory,
            IValidationExecutor validationExecutor,
            IProviderTypeService providerTypeService,
            IMessageService messageService,
            IIntegrationPointRepository integrationPointRepository,
            IRelativityObjectManager objectManager,
            ITaskParametersBuilder taskParametersBuilder,
            IRelativitySyncConstrainsChecker relativitySyncConstrainsChecker,
            IRelativitySyncAppIntegration relativitySyncAppIntegration)
            : base(helper, context, choiceQuery, serializer, managerFactory, validationExecutor, objectManager)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointService>();
            _jobManager = jobManager;
            _jobHistoryService = jobHistoryService;
            _jobHistoryErrorService = jobHistoryErrorService;
            _providerTypeService = providerTypeService;
            _messageService = messageService;
            _validationExecutor = validationExecutor;
            _integrationPointRepository = integrationPointRepository;
            _taskParametersBuilder = taskParametersBuilder;
            _relativitySyncConstrainsChecker = relativitySyncConstrainsChecker;
            _relativitySyncAppIntegration = relativitySyncAppIntegration;
        }

        protected override string UnableToSaveFormat
            => "Unable to save Integration Point:{0} cannot be changed once the Integration Point has been run";

        public IList<Data.IntegrationPoint> GetAllRDOs()
        {
            return _integrationPointRepository.GetAllIntegrationPoints();
        }

        public IList<Data.IntegrationPoint> GetAllRDOsWithAllFields()
        {
            return _integrationPointRepository.GetIntegrationPointsWithAllFields();
        }

        public virtual IntegrationPointModel ReadIntegrationPointModel(int artifactID)
        {
            Data.IntegrationPoint integrationPoint = ReadIntegrationPoint(artifactID);
            IntegrationPointModel integrationModel = IntegrationPointModel.FromIntegrationPoint(integrationPoint);
            return integrationModel;
        }

        public Data.IntegrationPoint ReadIntegrationPoint(int artifactID)
        {
            return _integrationPointRepository.ReadWithFieldMappingAsync(artifactID).GetAwaiter().GetResult();
        }

        public int SaveIntegration(IntegrationPointModel model)
        {
            try
            {
                if (model.ArtifactID > 0)
                {
                    IntegrationPointModel existingModel;
                    try
                    {
                        existingModel = ReadIntegrationPointModel(model.ArtifactID);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to save Integration Point: Unable to retrieve Integration Point", e);
                    }

                    if (existingModel.LastRun.HasValue)
                    {
                        ValidateConfigurationWhenUpdatingObject(model, existingModel);
                        model.HasErrors = existingModel.HasErrors;
                        model.LastRun = existingModel.LastRun;
                    }
                }

                IList<ChoiceRef> choices =
                    ChoiceQuery.GetChoicesOnField(Context.WorkspaceID, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));

                PeriodicScheduleRule rule = ConvertModelToScheduleRule(model);
                Data.IntegrationPoint integrationPoint = model.ToRdo(choices, rule);

                IntegrationPointModel integrationPointModel = IntegrationPointModel.FromIntegrationPoint(integrationPoint);

                SourceProvider sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
                DestinationProvider destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
                IntegrationPointType integrationPointType = GetIntegrationPointType(integrationPoint.Type);

                RunValidation(
                    integrationPointModel,
                    sourceProvider,
                    destinationProvider,
                    integrationPointType,
                    ObjectTypeGuids.IntegrationPointGuid);

                integrationPoint.ArtifactId = _integrationPointRepository.CreateOrUpdate(integrationPoint);

                TaskType task = GetJobTaskType(sourceProvider, destinationProvider);

                if (integrationPoint.EnableScheduler.GetValueOrDefault(false))
                {
                    var taskParameters = new TaskParameters()
                    {
                        BatchInstance = Guid.NewGuid()
                    };
                    _jobManager.CreateJob(taskParameters, task, Context.WorkspaceID, integrationPoint.ArtifactId, rule);
                }
                else
                {
                    Job job = _jobManager.GetJob(Context.WorkspaceID, integrationPoint.ArtifactId, task.ToString());
                    if (job != null)
                    {
                        _jobManager.DeleteJob(job.JobId);
                    }
                }

                return integrationPoint.ArtifactId;
            }
            catch (PermissionException ex)
            {
                CreateRelativityError(
                    Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_MESSAGE,
                    $"{Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_FULLTEXT_PREFIX}{Environment.NewLine}{ex.Message}");
                throw;
            }
            catch (IntegrationPointValidationException validationException)
            {
                CreateRelativityError(
                    Constants.IntegrationPoints.UNABLE_TO_SAVE_INTEGRATION_POINT_VALIDATION_FAILED,
                    string.Join(Environment.NewLine, validationException.ValidationResult.MessageTexts)
                );
                throw;
            }
            catch (Exception exception)
            {
                CreateRelativityError(
                    Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_ADMIN_MESSAGE,
                    String.Join(Environment.NewLine, new[] { exception.Message, exception.StackTrace })
                );

                throw new Exception(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE, exception);
            }
        }

        public void UpdateIntegrationPoint(Data.IntegrationPoint integrationPoint)
        {
            _integrationPointRepository.Update(integrationPoint);
        }

        public void RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
        {
            Data.IntegrationPoint integrationPoint;
            SourceProvider sourceProvider;
            DestinationProvider destinationProvider;

            _logger.LogInformation("Run Integration Point {integrationPointId} was requested...", integrationPointArtifactId);

            try
            {
                integrationPoint = _integrationPointRepository.ReadWithFieldMappingAsync(integrationPointArtifactId).GetAwaiter().GetResult();
                sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
                destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
            }
            catch (Exception e)
            {
                CreateRelativityError(
                    Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
                    string.Join(Environment.NewLine, e.Message, e.StackTrace));

                throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_USER_MESSAGE);
            }

            Guid jobRunId = Guid.NewGuid();

            Data.JobHistory jobHistory = CreateJobHistory(integrationPoint, jobRunId, JobTypeChoices.JobHistoryRun);

            ValidateIntegrationPointBeforeRun(integrationPointArtifactId, userId, integrationPoint, sourceProvider, destinationProvider, jobHistory);

            bool shouldUseRelativitySyncAppIntegration = _relativitySyncConstrainsChecker.ShouldUseRelativitySyncApp(integrationPointArtifactId);

            if (shouldUseRelativitySyncAppIntegration)
            {
                _logger.LogInformation("Using Sync application to execute the job");
                _relativitySyncAppIntegration.SubmitSyncJobAsync(workspaceArtifactId, integrationPointArtifactId, jobHistory.ArtifactId, userId).GetAwaiter().GetResult();
                _logger.LogInformation("Sync job has been submitted");
            }
            else
            {
                _logger.LogInformation("Using Sync DLL to execute the job");
                CreateJob(integrationPoint, sourceProvider, destinationProvider, jobRunId, workspaceArtifactId, userId);
                _logger.LogInformation("Run request was completed successfully and job has been added to Schedule Queue.");
            }
        }

        public void RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId, bool switchToAppendOverlayMode)
        {
            Data.IntegrationPoint integrationPoint;
            SourceProvider sourceProvider;
            DestinationProvider destinationProvider;

            _logger.LogInformation("Retry Integration Point {integrationPointId} was requested...", integrationPointArtifactId);

            try
            {
                integrationPoint = _integrationPointRepository.ReadWithFieldMappingAsync(integrationPointArtifactId).GetAwaiter().GetResult();
                sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
                destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
            }
            catch (Exception e)
            {
                CreateRelativityError(
                    Constants.IntegrationPoints.UNABLE_TO_RETRY_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
                    string.Join(Environment.NewLine, e.Message, e.StackTrace));

                throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRY_INTEGRATION_POINT_USER_MESSAGE);
            }

            ValidateIntegrationPointBeforeRetryErrors(workspaceArtifactId, integrationPointArtifactId, integrationPoint, sourceProvider);

            Guid jobRunId = Guid.NewGuid();

            Data.JobHistory jobHistory = CreateJobHistory(integrationPoint, jobRunId, JobTypeChoices.JobHistoryRetryErrors, switchToAppendOverlayMode);

            ValidateIntegrationPointBeforeRun(integrationPointArtifactId, userId, integrationPoint, sourceProvider, destinationProvider, jobHistory);

            CreateJob(integrationPoint, sourceProvider, destinationProvider, jobRunId, workspaceArtifactId, userId);

            _logger.LogInformation("Retry request was completed successfully and job has been added to Schedule Queue.");
        }

        public IEnumerable<FieldMap> GetFieldMap(int artifactID)
        {
            return _integrationPointRepository.GetFieldMappingAsync(artifactID).GetAwaiter().GetResult();
        }

        public void MarkIntegrationPointToStopJobs(int workspaceArtifactId, int integrationPointArtifactId)
        {
            CheckStopPermission(integrationPointArtifactId);

            IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager();
            StoppableJobHistoryCollection stoppableJobHistories = jobHistoryManager.GetStoppableJobHistory(workspaceArtifactId, integrationPointArtifactId);
            _logger.LogInformation("JobHistory requested for stopping {@jobHistoryToStop}", stoppableJobHistories);

            IDictionary<Guid, List<Job>> jobs = _jobManager.GetJobsByBatchInstanceId(integrationPointArtifactId);
            _logger.LogInformation("Jobs marked to stopping with correspondent BatchInstanceId {@jobs}", jobs);

            StopSyncAppJobs(stoppableJobHistories);

            List<Exception> exceptions = new List<Exception>();
            
            List<Data.JobHistory> processingJobHistories = stoppableJobHistories.ProcessingJobHistory.Where(x => !FilterSyncAppJobHistory(x)).ToList();
            foreach (Data.JobHistory jobHistory in processingJobHistories)
            {
                try
                {
                    Guid batchInstance = Guid.Parse(jobHistory.BatchInstance);

                    if (jobs.ContainsKey(batchInstance))
                    {
                        IList<long> jobIdsForGivenJobHistory = jobs[batchInstance].Select(x => x.JobId).ToList();
                        _jobManager.StopJobs(jobIdsForGivenJobHistory);
                        _logger.LogInformation("Jobs {@jobs} has been marked to stop for {jobHistoryId}", jobIdsForGivenJobHistory, jobHistory.ArtifactId);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    _logger.LogError(ex, "Error occurred when stopping Jobs for Processing JobHistory {jobHistoryId}", jobHistory.ArtifactId);
                }
            }

            List<Data.JobHistory> pendingJobHistories = stoppableJobHistories.PendingJobHistory.Where(x => !FilterSyncAppJobHistory(x)).ToList();
            foreach (Data.JobHistory jobHistory in pendingJobHistories)
            {
                try
                {
                    jobHistory.JobStatus = JobStatusChoices.JobHistoryStopped;
                    _jobHistoryService.UpdateRdo(jobHistory);

                    Guid batchInstance = Guid.Parse(jobHistory.BatchInstance);

                    if (jobs.ContainsKey(batchInstance))
                    {
                        jobs[batchInstance].ForEach(x => _jobManager.DeleteJob(x.JobId));

                        _logger.LogInformation("Jobs {@jobs} has been deleted from queue and JobHistory {jobHistoryId} was set to Stopped",
                            jobs[batchInstance], jobHistory.ArtifactId);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    _logger.LogError(ex, "Error occurred when deleteing Jobs and updating JobHistory {jobHistoryId} to Stopped", jobHistory.ArtifactId);
                }
            }

            if (exceptions.Any())
            {
                AggregateException stopActionException = new AggregateException(exceptions);
                _logger.LogError(stopActionException, "Errors occurred when stopping Integration Point {integrationPointId}", integrationPointArtifactId);
                throw stopActionException;
            }
        }

        private void StopSyncAppJobs(StoppableJobHistoryCollection stoppableJobHistories)
        {
            List<Data.JobHistory> syncAppJobHistories = stoppableJobHistories
                .PendingJobHistory
                .Concat(stoppableJobHistories.ProcessingJobHistory)
                .Where(FilterSyncAppJobHistory)
                .ToList();

            foreach (Data.JobHistory syncAppJobHistory in syncAppJobHistories)
            {
                try
                {
                    _relativitySyncAppIntegration.CancelJobAsync(Guid.Parse(syncAppJobHistory.JobID)).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cancel Sync Job ID: {jobId}", syncAppJobHistory.JobID);
                }
            }
        }

        private void CheckPreviousJobHistoryStatusOnRetry(int workspaceArtifactId, int integrationPointArtifactId)
        {
            _logger.LogInformation("Checking if last Job History for Integration Point {integrationPointId} is valid for retry",
                integrationPointArtifactId);

            Data.JobHistory lastJobHistory = null;
            try
            {
                IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager();
                int lastJobHistoryArtifactId = jobHistoryManager.GetLastJobHistoryArtifactId(workspaceArtifactId, integrationPointArtifactId);

                _logger.LogInformation("Last Job History for IntegrationPoint {integrationPointId} was retrieved - {lastJobHistoryId}",
                    integrationPointArtifactId, lastJobHistoryArtifactId);

                var request = new QueryRequest
                {
                    Condition = $"'ArtifactID' == {lastJobHistoryArtifactId}"
                };
                lastJobHistory = ObjectManager.Query<Data.JobHistory>(request).Single();

                _logger.LogInformation("Last Job History RDO for {lastJobHistoryId} was read.", lastJobHistoryArtifactId);
            }
            catch (Exception exception)
            {
                throw new Exception(Constants.IntegrationPoints.FAILED_TO_RETRIEVE_JOB_HISTORY, exception);
            }

            if (lastJobHistory == null)
            {
                throw new Exception(Constants.IntegrationPoints.FAILED_TO_RETRIEVE_JOB_HISTORY);
            }

            if (lastJobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopped))
            {
                throw new Exception(Constants.IntegrationPoints.RETRY_ON_STOPPED_JOB);
            }
        }

        private void CheckStopPermission(int integrationPointArtifactId)
        {
            Data.IntegrationPoint integrationPoint =
                _integrationPointRepository.ReadWithFieldMappingAsync(integrationPointArtifactId).GetAwaiter().GetResult();
            SourceProvider sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
            DestinationProvider destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
            IntegrationPointType integrationPointType = GetIntegrationPointType(integrationPoint.Type);

            var context = new ValidationContext
            {
                DestinationProvider = destinationProvider,
                IntegrationPointType = integrationPointType,
                Model = IntegrationPointModel.FromIntegrationPoint(integrationPoint),
                ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
                SourceProvider = sourceProvider,
                UserId = -1
            };

            try
            {
                _validationExecutor.ValidateOnStop(context);
            }
            catch (PermissionException ex)
            {
                CreateRelativityError(
                    Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
                    $"User is missing the following permissions:{Environment.NewLine}{String.Join(Environment.NewLine, ex.Message)}");
                throw;
            }
        }

        private void CreateJob(
            Data.IntegrationPoint integrationPoint,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            Guid jobRunId,
            int workspaceArtifactId,
            int userId)
        {
            _logger.LogInformation("Creating Job for Integration Point {integrationPointId} by user {userId}...",
                integrationPoint.ArtifactId, userId);

            lock (Lock)
            {
                // If the Relativity provider is selected, we need to create an export task
                TaskType jobTaskType = GetJobTaskType(sourceProvider, destinationProvider);

                CheckForOtherJobsExecutingOrInQueue(jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId);

                TaskParameters jobDetails = _taskParametersBuilder.Build(jobTaskType, jobRunId, integrationPoint);

                _jobManager.CreateJobOnBehalfOfAUser(jobDetails, jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId, userId);
            }

            _logger.LogInformation("Job was successfully created.");
        }

        private Data.JobHistory CreateJobHistory(Data.IntegrationPoint integrationPoint, Guid jobRunId, ChoiceRef jobType, bool switchToAppendOverlayMode = false)
        {
            _logger.LogInformation("Creating Job History for Integration Point {integrationPointId} with BatchInstance {batchInstance}...",
                integrationPoint.ArtifactId, jobRunId);

            Data.JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, jobRunId, jobType, null);
            AdjustOverwriteModeForRetry(jobHistory, switchToAppendOverlayMode);

            if (jobHistory == null)
            {
                _logger.LogError(Constants.IntegrationPoints.FAILED_TO_CREATE_JOB_HISTORY);
                throw new Exception(Constants.IntegrationPoints.FAILED_TO_CREATE_JOB_HISTORY);
            }

            _logger.LogInformation("Job History {jobHistoryId} was created for Integration Point {integrationPointId}.",
                jobHistory.ArtifactId, integrationPoint.ArtifactId);

            return jobHistory;
        }

        private void AdjustOverwriteModeForRetry(Data.JobHistory jobHistory, bool switchToAppendOverlayMode)
        {
            if (switchToAppendOverlayMode)
            {
                jobHistory.Overwrite = OverwriteFieldsChoices.IntegrationPointAppendOverlay.Name;
                _jobHistoryService.UpdateRdo(jobHistory);
            }
        }

        private void SetJobHistoryStatus(Data.JobHistory jobHistory, ChoiceRef status)
        {
            if (jobHistory != null)
            {
                jobHistory.JobStatus = status;
                _jobHistoryService.UpdateRdo(jobHistory);
            }
            else
            {
                _logger.LogWarning("Unable to set JobHistory status - jobHistory object is null.");
            }
        }

        private TaskType GetJobTaskType(SourceProvider sourceProvider, DestinationProvider destinationProvider)
        {
            //The check on the destinationProvider should come first in the if block.
            //If destProvider is load file, it should be ExportManager type no matter what the sourceProvider is.
            if (destinationProvider.Identifier.Equals(Synchronizer.RdoSynchronizerProvider.FILES_SYNC_TYPE_GUID))
            {
                return TaskType.ExportManager;
            }
            if (sourceProvider.Identifier.Equals(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
            {
                return TaskType.ExportService;
            }
            if (sourceProvider.Identifier.Equals(Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE))
            {
                return TaskType.ImportService;
            }

            return TaskType.SyncManager;
        }

        private void CheckForOtherJobsExecutingOrInQueue(TaskType taskType, int workspaceArtifactId, int integrationPointArtifactId)
        {
            if (taskType == TaskType.ExportService || taskType == TaskType.SyncManager || taskType == TaskType.ExportManager)
            {
                IQueueManager queueManager = ManagerFactory.CreateQueueManager();
                bool jobsExecutingOrInQueue = queueManager.HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

                if (jobsExecutingOrInQueue)
                {
                    throw new Exception(Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);
                }
            }
        }

        private void HandleValidationError(Data.JobHistory jobHistory, int integrationPointArtifactId, Data.IntegrationPoint integrationPoint, Exception ex)
        {
            AddValidationErrorToJobHistory(jobHistory, ex);
            AddValidationErrorToErrorTab(ex);
            SetJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryValidationFailed);
            SetHasErrorOnIntegrationPoint(integrationPointArtifactId);
            SendValidationFailedMessage(integrationPoint, jobHistory.BatchInstance);
        }

        private void SendValidationFailedMessage(Data.IntegrationPoint integrationPoint, string batchInstance)
        {
            _messageService.Send(new JobValidationFailedMessage
            {
                Provider = integrationPoint.GetProviderType(_providerTypeService).ToString(),
                CorrelationID = batchInstance
            });
        }

        private void AddValidationErrorToJobHistory(Data.JobHistory jobHistory, Exception ex)
        {
            string errorMessage = GetValidationErrorMessage(ex);

            _jobHistoryErrorService.JobHistory = jobHistory;
            _jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, errorMessage, string.Empty);
            _jobHistoryErrorService.CommitErrors();
        }

        private void AddValidationErrorToErrorTab(Exception ex)
        {
            CreateRelativityError(_VALIDATION_FAILED, ex.Message);
        }

        private void SetHasErrorOnIntegrationPoint(int integrationPointArtifactId)
        {
            Data.IntegrationPoint integrationPoint =
                _integrationPointRepository.ReadWithFieldMappingAsync(integrationPointArtifactId).GetAwaiter().GetResult();
            integrationPoint.HasErrors = true;
            _integrationPointRepository.Update(integrationPoint);
        }

        private static string GetValidationErrorMessage(Exception ex)
        {
            string errorMessage;
            var aggregatedException = ex as AggregateException;
            if (aggregatedException != null)
            {
                IEnumerable<string> innerMessages = aggregatedException.InnerExceptions.Select(x => x.Message);
                errorMessage = $"{aggregatedException.Message} : {string.Join(",", innerMessages)}";
            }
            else
            {
                errorMessage = ex.Message;
            }

            return errorMessage;
        }

        private void ValidateIntegrationPointBeforeRun(
            int integrationPointArtifactId,
            int userId,
            Data.IntegrationPoint integrationPoint,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            Data.JobHistory jobHistory)
        {
            try
            {
                _logger.LogInformation("Integration Point {integrationPointId} validating...", integrationPointArtifactId);

                IntegrationPointType integrationPointType = GetIntegrationPointType(integrationPoint.Type);
                IntegrationPointModel model = IntegrationPointModel.FromIntegrationPoint(integrationPoint);

                var context = new ValidationContext
                {
                    DestinationProvider = destinationProvider,
                    IntegrationPointType = integrationPointType,
                    Model = model,
                    ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
                    SourceProvider = sourceProvider,
                    UserId = userId
                };

                _validationExecutor.ValidateOnRun(context);

                _logger.LogInformation("Integration Point {integrationPointId} was successfully validated.", integrationPointArtifactId);
            }
            catch (Exception ex)
            {
                HandleValidationError(jobHistory, integrationPointArtifactId, integrationPoint, ex);
                throw;
            }
        }

        private void ValidateIntegrationPointBeforeRetryErrors(int workspaceArtifactId,
            int integrationPointArtifactId, Data.IntegrationPoint integrationPoint, SourceProvider sourceProvider)
        {
            if (!sourceProvider.Identifier.Equals(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception(Constants.IntegrationPoints.RETRY_IS_NOT_RELATIVITY_PROVIDER);
            }

            CheckPreviousJobHistoryStatusOnRetry(workspaceArtifactId, integrationPointArtifactId);

            if (integrationPoint.HasErrors.HasValue == false || integrationPoint.HasErrors.Value == false)
            {
                throw new Exception(Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);
            }
        }

        private bool FilterSyncAppJobHistory(Data.JobHistory jobHistory)
        {
            // When Job ID is a valid GUID then we know it's a Sync App Job
            return Guid.TryParse(jobHistory.JobID, out Guid jobId);
        }
    }
}
