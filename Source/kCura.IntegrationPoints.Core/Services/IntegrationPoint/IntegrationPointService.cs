using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.RelativitySync;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.RelativitySync;
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
        private readonly IRetryHandler _retryHandler;

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

            _retryHandler = new RetryHandlerFactory(_logger).Create();
        }

        protected override string UnableToSaveFormat
            => "Unable to save Integration Point:{0} cannot be changed once the Integration Point has been run";

        public IntegrationPointSlimDto ReadSlim(int artifactID)
        {
            Data.IntegrationPoint integrationPoint = _integrationPointRepository.ReadAsync(artifactID).GetAwaiter().GetResult();
            return ToSlim(integrationPoint);
        }

        public IntegrationPointDto Read(int artifactID)
         {
            Data.IntegrationPoint integrationPoint = _integrationPointRepository.ReadAsync(artifactID).GetAwaiter().GetResult();
            IntegrationPointDto dto = ToDto(integrationPoint);
            dto.FieldMappings = GetFieldMap(artifactID);
            dto.SourceConfiguration = _integrationPointRepository.GetSourceConfigurationAsync(artifactID).GetAwaiter().GetResult();
            dto.DestinationConfiguration = _integrationPointRepository.GetDestinationConfigurationAsync(artifactID).GetAwaiter().GetResult();
            return dto;
        }

        public List<IntegrationPointSlimDto> ReadAllSlim()
        {
            return _integrationPointRepository
                .ReadAll()
                .Select(ToSlim)
                .ToList();
        }

        public List<IntegrationPointDto> ReadAll()
        {
            List<IntegrationPointDto> dtoList = _integrationPointRepository
                .ReadAll()
                .Select(ToDto)
                .ToList();

            foreach (var dto in dtoList)
            {
                dto.FieldMappings = GetFieldMap(dto.ArtifactId);
                dto.SourceConfiguration = _integrationPointRepository.GetSourceConfigurationAsync(dto.ArtifactId).GetAwaiter().GetResult();
                dto.DestinationConfiguration = _integrationPointRepository.GetDestinationConfigurationAsync(dto.ArtifactId).GetAwaiter().GetResult();
            }

            return dtoList;
        }

        public List<FieldMap> GetFieldMap(int artifactId)
        {
            return _retryHandler.Execute<List<FieldMap>, RipSerializationException>(
                ReadFieldMapping,
                exception =>
                {
                    _logger.LogWarning(
                        exception,
                        "Unable to deserialize field mapping for integration point: {integrationPointId}. Mapping value: {fieldMapping}. Operation will be retried.",
                        artifactId,
                        exception.Value);
                });

            List<FieldMap> ReadFieldMapping()
            {
                string fieldMapString = _integrationPointRepository.GetFieldMappingAsync(artifactId).GetAwaiter().GetResult();
                List<FieldMap> fieldMap = Serializer.Deserialize<List<FieldMap>>(fieldMapString);
                SanitizeFieldsMapping(fieldMap);
                return fieldMap;
            }
        }

        public List<IntegrationPointSlimDto> GetBySourceAndDestinationProvider(int sourceProviderArtifactID, int destinationProviderArtifactID)
        {
            List<Data.IntegrationPoint> rdos = _integrationPointRepository
                .ReadBySourceAndDestinationProviderAsync(sourceProviderArtifactID, destinationProviderArtifactID)
                .GetAwaiter()
                .GetResult();

            return rdos.Select(ToSlim).ToList();
        }

        public int SaveIntegrationPoint(IntegrationPointDto dto)
        {
            try
            {
                if (dto.ArtifactId > 0)
                {
                    IntegrationPointDto existingDto;
                    try
                    {
                        existingDto = Read(dto.ArtifactId);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to save Integration Point: Unable to retrieve Integration Point", e);
                    }

                    if (existingDto.LastRun.HasValue)
                    {
                        ValidateConfigurationWhenUpdatingObject(dto, existingDto);
                        dto.HasErrors = existingDto.HasErrors;
                        dto.LastRun = existingDto.LastRun;
                    }
                }

                IList<ChoiceRef> choices = ChoiceQuery.GetChoicesOnField(Context.WorkspaceID, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));
                PeriodicScheduleRule rule = ConvertModelToScheduleRule(dto);

                SourceProvider sourceProvider = GetSourceProvider(dto.SourceProvider);
                DestinationProvider destinationProvider = GetDestinationProvider(dto.DestinationProvider);
                IntegrationPointType integrationPointType = GetIntegrationPointType(dto.Type);

                RunValidation(
                    dto,
                    sourceProvider,
                    destinationProvider,
                    integrationPointType,
                    ObjectTypeGuids.IntegrationPointGuid);

                Data.IntegrationPoint integrationPoint = ToRdo(dto, choices, rule);
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

        public void UpdateLastAndNextRunTime(int artifactId, DateTime? lastRuntime, DateTime? nextRuntime)
        {
            Data.IntegrationPoint integrationPoint = _integrationPointRepository.ReadAsync(artifactId).GetAwaiter().GetResult();
            integrationPoint.LastRuntimeUTC = lastRuntime;
            integrationPoint.NextScheduledRuntimeUTC = nextRuntime;
            _integrationPointRepository.Update(integrationPoint);
        }

        public void DisableScheduler(int artifactId)
        {
            Data.IntegrationPoint integrationPoint = _integrationPointRepository.ReadAsync(artifactId).GetAwaiter().GetResult();
            integrationPoint.ScheduleRule = null;
            integrationPoint.NextScheduledRuntimeUTC = null;
            integrationPoint.EnableScheduler = false;
            _integrationPointRepository.Update(integrationPoint);
        }

        public void UpdateJobHistory(int artifactId, List<int> jobHistory)
        {
            Data.IntegrationPoint integrationPoint = _integrationPointRepository.ReadAsync(artifactId).GetAwaiter().GetResult();
            integrationPoint.JobHistory = jobHistory.ToArray();
            _integrationPointRepository.Update(integrationPoint);
        }

        public void UpdateConfiguration(int artifactId, string sourceConfiguration, string destinationConfiguration)
        {
            Data.IntegrationPoint integrationPoint = _integrationPointRepository.ReadAsync(artifactId).GetAwaiter().GetResult();
            integrationPoint.SourceConfiguration = sourceConfiguration;
            integrationPoint.DestinationConfiguration = destinationConfiguration;
            _integrationPointRepository.Update(integrationPoint);
        }

        public void RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
        {
            IntegrationPointDto integrationPointDto;
            SourceProvider sourceProvider;
            DestinationProvider destinationProvider;

            _logger.LogInformation("Run Integration Point {integrationPointId} was requested...", integrationPointArtifactId);

            try
            {
                integrationPointDto = Read(integrationPointArtifactId);
                sourceProvider = GetSourceProvider(integrationPointDto.SourceProvider);
                destinationProvider = GetDestinationProvider(integrationPointDto.DestinationProvider);
            }
            catch (Exception e)
            {
                CreateRelativityError(
                    Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
                    string.Join(Environment.NewLine, e.Message, e.StackTrace));

                throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_USER_MESSAGE);
            }

            Guid jobRunId = Guid.NewGuid();
            Data.JobHistory jobHistory = CreateJobHistory(integrationPointDto, jobRunId, JobTypeChoices.JobHistoryRun);

            ValidateIntegrationPointBeforeRun(userId, integrationPointDto, sourceProvider, destinationProvider, jobHistory);

            bool shouldUseRelativitySyncAppIntegration = _relativitySyncConstrainsChecker.ShouldUseRelativitySyncApp(integrationPointArtifactId);

            if (shouldUseRelativitySyncAppIntegration)
            {
                _logger.LogInformation("Using Sync application to execute the job");
                _relativitySyncAppIntegration.SubmitSyncJobAsync(workspaceArtifactId, integrationPointDto, jobHistory.ArtifactId, userId).GetAwaiter().GetResult();
                _logger.LogInformation("Sync job has been submitted");
            }
            else
            {
                _logger.LogInformation("Using Sync DLL to execute the job");
                CreateJob(integrationPointDto, sourceProvider, destinationProvider, jobRunId, workspaceArtifactId, userId);
                _logger.LogInformation("Run request was completed successfully and job has been added to Schedule Queue.");
            }
        }

        public void RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId, bool switchToAppendOverlayMode)
        {
            IntegrationPointDto integrationPointDto;
            SourceProvider sourceProvider;
            DestinationProvider destinationProvider;

            _logger.LogInformation("Retry Integration Point {integrationPointId} was requested...", integrationPointArtifactId);

            try
            {
                integrationPointDto = Read(integrationPointArtifactId);
                sourceProvider = GetSourceProvider(integrationPointDto.SourceProvider);
                destinationProvider = GetDestinationProvider(integrationPointDto.DestinationProvider);
            }
            catch (Exception e)
            {
                CreateRelativityError(
                    Constants.IntegrationPoints.UNABLE_TO_RETRY_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
                    string.Join(Environment.NewLine, e.Message, e.StackTrace));

                throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRY_INTEGRATION_POINT_USER_MESSAGE);
            }

            ValidateIntegrationPointBeforeRetryErrors(workspaceArtifactId, integrationPointArtifactId, integrationPointDto, sourceProvider);

            Guid jobRunId = Guid.NewGuid();

            Data.JobHistory jobHistory = CreateJobHistory(integrationPointDto, jobRunId, JobTypeChoices.JobHistoryRetryErrors, switchToAppendOverlayMode);

            ValidateIntegrationPointBeforeRun(userId, integrationPointDto, sourceProvider, destinationProvider, jobHistory);

            CreateJob(integrationPointDto, sourceProvider, destinationProvider, jobRunId, workspaceArtifactId, userId);

            _logger.LogInformation("Retry request was completed successfully and job has been added to Schedule Queue.");
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
            IntegrationPointDto dto = Read(integrationPointArtifactId);
            SourceProvider sourceProvider = GetSourceProvider(dto.SourceProvider);
            DestinationProvider destinationProvider = GetDestinationProvider(dto.DestinationProvider);
            IntegrationPointType integrationPointType = GetIntegrationPointType(dto.Type);

            var context = new ValidationContext
            {
                DestinationProvider = destinationProvider,
                IntegrationPointType = integrationPointType,
                Model = dto,
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
            IntegrationPointDto integrationPoint,
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

                TaskParameters jobDetails = _taskParametersBuilder.Build(jobTaskType, jobRunId, integrationPoint.SourceConfiguration, integrationPoint.DestinationConfiguration);

                _jobManager.CreateJobOnBehalfOfAUser(jobDetails, jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId, userId);
            }

            _logger.LogInformation("Job was successfully created.");
        }

        private Data.JobHistory CreateJobHistory(IntegrationPointDto integrationPointDto, Guid jobRunId, ChoiceRef jobType, bool switchToAppendOverlayMode = false)
        {
            _logger.LogInformation("Creating Job History for Integration Point {integrationPointId} with BatchInstance {batchInstance}...",
                integrationPointDto.ArtifactId, jobRunId);

            Data.JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPointDto, jobRunId, jobType, null);
            AdjustOverwriteModeForRetry(jobHistory, switchToAppendOverlayMode);

            if (jobHistory == null)
            {
                _logger.LogError(Constants.IntegrationPoints.FAILED_TO_CREATE_JOB_HISTORY);
                throw new Exception(Constants.IntegrationPoints.FAILED_TO_CREATE_JOB_HISTORY);
            }

            _logger.LogInformation("Job History {jobHistoryId} was created for Integration Point {integrationPointId}.",
                jobHistory.ArtifactId, integrationPointDto.ArtifactId);

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
            // The check on the destinationProvider should come first in the if block.
            // If destProvider is load file, it should be ExportManager type no matter what the sourceProvider is.
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

        private void HandleValidationError(Data.JobHistory jobHistory, IntegrationPointDto integrationPointDto, Exception ex)
        {
            AddValidationErrorToJobHistory(jobHistory, ex);
            AddValidationErrorToErrorTab(ex);
            SetJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryValidationFailed);
            SetHasErrorOnIntegrationPoint(integrationPointDto.ArtifactId);
            SendValidationFailedMessage(integrationPointDto, jobHistory.BatchInstance);
        }

        private void SendValidationFailedMessage(IntegrationPointDto integrationPointDto, string batchInstance)
        {
            _messageService.Send(new JobValidationFailedMessage
            {
                Provider = integrationPointDto.GetProviderType(_providerTypeService).ToString(),
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
            Data.IntegrationPoint integrationPoint =  _integrationPointRepository.ReadAsync(integrationPointArtifactId).GetAwaiter().GetResult();
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
            int userId,
            IntegrationPointDto integrationPointDto,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            Data.JobHistory jobHistory)
        {
            try
            {
                _logger.LogInformation("Integration Point {integrationPointId} validating...", integrationPointDto.ArtifactId);

                IntegrationPointType integrationPointType = GetIntegrationPointType(integrationPointDto.Type);

                var context = new ValidationContext
                {
                    DestinationProvider = destinationProvider,
                    IntegrationPointType = integrationPointType,
                    Model = integrationPointDto,
                    ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
                    SourceProvider = sourceProvider,
                    UserId = userId
                };

                _validationExecutor.ValidateOnRun(context);

                _logger.LogInformation("Integration Point {integrationPointId} was successfully validated.", integrationPointDto.ArtifactId);
            }
            catch (Exception ex)
            {
                HandleValidationError(jobHistory, integrationPointDto, ex);
                throw;
            }
        }

        private void ValidateIntegrationPointBeforeRetryErrors(
            int workspaceArtifactId,
            int integrationPointArtifactId,
            IntegrationPointDto integrationPoint,
            SourceProvider sourceProvider)
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

        private IntegrationPointDto ToDto(Data.IntegrationPoint rdo)
        {
            var dto = new IntegrationPointDto
            {
                ArtifactId = rdo.ArtifactId,
                Name = rdo.Name,
                SelectedOverwrite = rdo.OverwriteFields == null ? string.Empty : rdo.OverwriteFields.Name,
                SourceProvider = rdo.SourceProvider.GetValueOrDefault(0),
                DestinationConfiguration = rdo.DestinationConfiguration,
                SourceConfiguration = rdo.SourceConfiguration,
                DestinationProvider = rdo.DestinationProvider.GetValueOrDefault(0),
                Type = rdo.Type.GetValueOrDefault(0),
                Scheduler = new Scheduler(rdo.EnableScheduler.GetValueOrDefault(false), rdo.ScheduleRule),
                EmailNotificationRecipients = rdo.EmailNotificationRecipients ?? string.Empty,
                LogErrors = rdo.LogErrors.GetValueOrDefault(false),
                HasErrors = rdo.HasErrors.GetValueOrDefault(false),
                LastRun = rdo.LastRuntimeUTC,
                NextRun = rdo.NextScheduledRuntimeUTC,
                SecuredConfiguration = rdo.SecuredConfiguration,
                JobHistory = rdo.JobHistory.ToList(),
                FieldMappings = Serializer.Deserialize<List<FieldMap>>(rdo.FieldMappings ?? string.Empty),
            };
            SanitizeFieldsMapping(dto.FieldMappings);
            return dto;
        }

        private IntegrationPointSlimDto ToSlim(Data.IntegrationPoint rdo)
        {
            return new IntegrationPointSlimDto
            {
                ArtifactId = rdo.ArtifactId,
                Name = rdo.Name,
                SelectedOverwrite = rdo.OverwriteFields == null ? string.Empty : rdo.OverwriteFields.Name,
                SourceProvider = rdo.SourceProvider.GetValueOrDefault(0),
                DestinationProvider = rdo.DestinationProvider.GetValueOrDefault(0),
                Type = rdo.Type.GetValueOrDefault(0),
                Scheduler = new Scheduler(rdo.EnableScheduler.GetValueOrDefault(false), rdo.ScheduleRule),
                EmailNotificationRecipients = rdo.EmailNotificationRecipients ?? string.Empty,
                LogErrors = rdo.LogErrors.GetValueOrDefault(false),
                HasErrors = rdo.HasErrors.GetValueOrDefault(false),
                LastRun = rdo.LastRuntimeUTC,
                NextRun = rdo.NextScheduledRuntimeUTC,
                SecuredConfiguration = rdo.SecuredConfiguration,
                JobHistory = rdo.JobHistory.ToList(),
            };
        }

        private Data.IntegrationPoint ToRdo(IntegrationPointDto dto, IEnumerable<ChoiceRef> choices, PeriodicScheduleRule rule)
        {
            ChoiceRef choice = choices.FirstOrDefault(x => x.Name.Equals(dto.SelectedOverwrite));
            if (choice == null)
            {
                throw new Exception("Cannot find choice by the name " + dto.SelectedOverwrite);
            }

            var IntegrationPointRdo = new Data.IntegrationPoint
            {
                ArtifactId = dto.ArtifactId,
                Name = dto.Name,
                OverwriteFields = new ChoiceRef(choice.ArtifactID) {Name = choice.Name},
                SourceConfiguration = dto.SourceConfiguration,
                SourceProvider = dto.SourceProvider,
                Type = dto.Type,
                DestinationConfiguration = dto.DestinationConfiguration,
                FieldMappings = Serializer.Serialize(dto.FieldMappings ?? new List<FieldMap>()),
                EnableScheduler = dto.Scheduler.EnableScheduler,
                DestinationProvider = dto.DestinationProvider,
                LogErrors = dto.LogErrors,
                HasErrors = dto.HasErrors,
                EmailNotificationRecipients =
                    string.Join("; ", (dto.EmailNotificationRecipients ?? string.Empty).Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList()),
                LastRuntimeUTC = dto.LastRun,
                SecuredConfiguration = dto.SecuredConfiguration,
                JobHistory = dto.JobHistory?.ToArray(),
            };

            if (IntegrationPointRdo.EnableScheduler.GetValueOrDefault(false))
            {
                IntegrationPointRdo.ScheduleRule = rule.ToSerializedString();
                IntegrationPointRdo.NextScheduledRuntimeUTC = rule.GetNextUTCRunDateTime();
            }
            else
            {
                IntegrationPointRdo.ScheduleRule = string.Empty;
                IntegrationPointRdo.NextScheduledRuntimeUTC = null;
            }

            return IntegrationPointRdo;
        }

        private bool FilterSyncAppJobHistory(Data.JobHistory jobHistory)
        {
            // When Job ID is a valid GUID then we know it's a Sync App Job
            return Guid.TryParse(jobHistory.JobID, out Guid jobId);
        }
    }
}
