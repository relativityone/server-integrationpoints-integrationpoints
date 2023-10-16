using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Common.RelativitySync;
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
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.DataTransfer.MessageService;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Exceptions;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public class IntegrationPointService : IntegrationPointServiceBase, IIntegrationPointService
    {
        private const string _VALIDATION_FAILED = "Failed to submit integration job. Integration Point validation failed.";
        private readonly ILogger<IntegrationPointService> _logger;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobManager _jobManager;
        private readonly IMessageService _messageService;
        private readonly IProviderTypeService _providerTypeService;
        private readonly IIntegrationPointRepository _integrationPointRepository;
        private readonly IRelativityObjectManager _objectManager;
        private readonly ITaskParametersBuilder _taskParametersBuilder;
        private readonly IRelativitySyncConstrainsChecker _relativitySyncConstrainsChecker;
        private readonly IRelativitySyncAppIntegration _relativitySyncAppIntegration;
        private readonly IRetryHandler _retryHandler;
        private readonly IAgentLauncher _agentLauncher;
        private readonly IDateTimeHelper _dateTimeHelper;

        public IntegrationPointService(
            ICaseServiceContext context,
            ISerializer serializer,
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
            IRelativitySyncAppIntegration relativitySyncAppIntegration,
            IAgentLauncher agentLauncher,
            IDateTimeHelper dateTimeHelper,
            IRetryHandler retryHandler,
            ILogger<IntegrationPointService> logger)
            : base(context, choiceQuery, serializer, managerFactory, validationExecutor, objectManager)
        {
            _logger = logger;
            _jobManager = jobManager;
            _jobHistoryService = jobHistoryService;
            _jobHistoryErrorService = jobHistoryErrorService;
            _providerTypeService = providerTypeService;
            _messageService = messageService;
            _validationExecutor = validationExecutor;
            _integrationPointRepository = integrationPointRepository;
            _objectManager = objectManager;
            _taskParametersBuilder = taskParametersBuilder;
            _relativitySyncConstrainsChecker = relativitySyncConstrainsChecker;
            _relativitySyncAppIntegration = relativitySyncAppIntegration;
            _agentLauncher = agentLauncher;
            _dateTimeHelper = dateTimeHelper;
            _retryHandler = retryHandler;
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
            dto.DestinationConfiguration = GetDestinationConfiguration(artifactID);
            dto.SourceConfiguration = _integrationPointRepository.GetSourceConfigurationAsync(artifactID).GetAwaiter().GetResult();
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
                dto.SourceConfiguration = GetSourceConfiguration(dto.ArtifactId);
                dto.DestinationConfiguration = GetDestinationConfiguration(dto.ArtifactId);
            }

            return dtoList;
        }

        public List<FieldMap> GetFieldMap(int artifactId)
        {
            var fieldMap = ReadLongTextWithRetries<List<FieldMap>>(_integrationPointRepository.GetFieldMappingAsync, artifactId);

            // CustomProviders are based on SourceFields data, this line is needed for these providers to work properly
            fieldMap?.ForEach(map => map.SourceField.IsIdentifier = map.FieldMapType == FieldMapTypeEnum.Identifier);
            return fieldMap;
        }

        public string GetSourceConfiguration(int artifactId)
        {
            return _integrationPointRepository.GetSourceConfigurationAsync(artifactId).GetAwaiter().GetResult();
        }

        public DestinationConfiguration GetDestinationConfiguration(int artifactId)
        {
            return ReadLongTextWithRetries<DestinationConfiguration>(_integrationPointRepository.GetDestinationConfigurationAsync, artifactId);
        }

        public CalculationState GetCalculationState(int artifactId)
        {
            return ReadLongTextWithRetries<CalculationState>(_integrationPointRepository.GetCalculationStateAsync, artifactId);
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
                        dto.JobHistory = existingDto.JobHistory;
                        dto.HasErrors = existingDto.HasErrors;
                        dto.LastRun = existingDto.LastRun;
                    }
                }
                else
                {
                    dto.HasErrors = false;
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
                Guid correlationId = Guid.NewGuid();

                if (integrationPoint.EnableScheduler.GetValueOrDefault(false))
                {
                    var taskParameters = new TaskParameters()
                    {
                        BatchInstance = correlationId
                    };
                    _jobManager.CreateJob(taskParameters, task, correlationId.ToString(), Context.WorkspaceID, integrationPoint.ArtifactId, rule);
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
                    string.Join(Environment.NewLine, validationException.ValidationResult.MessageTexts));
                throw;
            }
            catch (Exception exception)
            {
                CreateRelativityError(
                    Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_ADMIN_MESSAGE,
                    string.Join(Environment.NewLine, new[] { exception.Message, exception.StackTrace }));

                throw new Exception(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE, exception);
            }
        }

        public void UpdateLastAndNextRunTime(int artifactId, DateTime? lastRuntime, DateTime? nextRuntime)
        {
            _integrationPointRepository.UpdateLastAndNextRunTime(artifactId, lastRuntime, nextRuntime);
        }

        public void DisableScheduler(int artifactId)
        {
            _integrationPointRepository.DisableScheduler(artifactId);
        }

        public void UpdateJobHistory(int artifactId, List<int> jobHistory)
        {
            _integrationPointRepository.UpdateJobHistory(artifactId, jobHistory);
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

            ValidateLastJobHistoryBeforeRun(workspaceArtifactId, integrationPointArtifactId);

            Guid batchInstance = Guid.NewGuid();
            Data.JobHistory jobHistory = CreateJobHistory(integrationPointDto, batchInstance, JobTypeChoices.JobHistoryRun);
            ValidateIntegrationPointBeforeRun(userId, integrationPointDto, sourceProvider, destinationProvider, jobHistory);
            SubmitJob(workspaceArtifactId, integrationPointArtifactId, userId, integrationPointDto, jobHistory, sourceProvider, destinationProvider, batchInstance.ToString());
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

            ProviderType providerType = _providerTypeService.GetProviderType(
                integrationPointDto.SourceProvider,
                integrationPointDto.DestinationProvider);

            ValidateLastJobHistoryBeforeRetry(workspaceArtifactId, integrationPointArtifactId, providerType);

            Guid batchInstance = Guid.NewGuid();
            Data.JobHistory jobHistory = CreateJobHistory(integrationPointDto, batchInstance, JobTypeChoices.JobHistoryRetryErrors, switchToAppendOverlayMode);
            ValidateIntegrationPointBeforeRun(userId, integrationPointDto, sourceProvider, destinationProvider, jobHistory);
            SubmitJob(workspaceArtifactId, integrationPointArtifactId, userId, integrationPointDto, jobHistory, sourceProvider, destinationProvider, batchInstance.ToString());
        }

        public void MarkIntegrationPointToStopJobs(int workspaceArtifactId, int integrationPointArtifactId)
        {
            CheckStopPermission(integrationPointArtifactId);

            IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager();
            Data.JobHistory jobHistory = jobHistoryManager.GetLastJobHistory(workspaceArtifactId, integrationPointArtifactId);
            _logger.LogInformation("JobHistory requested for stopping {@jobHistoryToStop}", jobHistory);

            if (jobHistory == null)
            {
                throw new NotFoundException($"Last Job History for workspaceArtifactId - {workspaceArtifactId}, integrationPointArtifactId - {integrationPointArtifactId}")
            }

            if (FilterSyncAppJobHistory(jobHistory))
            {
                StopSyncAppJobs(jobHistory);
                return;
            }

            Guid jobHistoryGuid = new Guid(jobHistory.BatchInstance);
            try
            {
                if (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending))
                {
                    StopPendingJob(integrationPointArtifactId, jobHistory, jobHistoryGuid);
                    return;
                }

                StopExecutingJob(integrationPointArtifactId, jobHistoryGuid);
            }
            catch (Exception ex)
            {
                string exceptionMessage = $"Errors occurred when stopping Integration Point integrationPointId - {integrationPointArtifactId}, jobHistoryGuid - {jobHistoryGuid}";
                throw new Exception(exceptionMessage, ex);
            }
        }

        private void SubmitJob(int workspaceArtifactId, int integrationPointArtifactId, int userId, IntegrationPointDto integrationPointDto, Data.JobHistory jobHistory, SourceProvider sourceProvider, DestinationProvider destinationProvider, string correlationID)
        {
            _logger.LogInformation("Submitting Job for Integration Point {integrationPointId} with JobHistoryId {jobHistoryId}", integrationPointArtifactId, jobHistory.ArtifactId);

            bool shouldUseRelativitySyncAppIntegration = _relativitySyncConstrainsChecker.ShouldUseRelativitySyncApp(integrationPointArtifactId);
            if (shouldUseRelativitySyncAppIntegration)
            {
                _logger.LogInformation("Using Sync application to run the job");
                try
                {
                    _relativitySyncAppIntegration.SubmitSyncJobAsync(workspaceArtifactId, integrationPointDto, jobHistory.ArtifactId, userId, correlationID).GetAwaiter().GetResult();
                }
                catch (SyncJobSendingException ex)
                {
                    _logger.LogError(ex, "Failed to send sync job");
                    MarkSyncJobAsFailed(jobHistory.ArtifactId, integrationPointArtifactId, ex);
                }
            }
            else
            {
                _logger.LogInformation("Adding Job to IntegrationPoints Queue...");
                Job job = CreateJob(integrationPointDto, sourceProvider, destinationProvider, correlationID, workspaceArtifactId, userId);
                if (job != null)
                {
                    _agentLauncher.LaunchAgentAsync().GetAwaiter().GetResult();
                    _logger.LogInformation("Run request was completed successfully and job has been added to Schedule Queue.");
                }
            }
        }

        private T ReadLongTextWithRetries<T>(Func<int, Task<string>> longTextAccessor, int integrationPointId)
        {
            return _retryHandler.Execute<T, RipSerializationException>(
                () =>
                {
                    string longTextString = longTextAccessor(integrationPointId).GetAwaiter().GetResult();
                    return Serializer.Deserialize<T>(longTextString);
                },
                exception =>
                {
                    _logger.LogWarning(
                        exception,
                        "Unable to deserialize {fieldType} for integration point: {integrationPointId}. LongText value: {longText}. Operation will be retried.",
                        typeof(T),
                        integrationPointId,
                        exception.Value ?? string.Empty);
                });
        }

        private void MarkSyncJobAsFailed(int jobHistoryId, int integrationPointId, Exception ex)
        {
            DateTime endTime = _dateTimeHelper.Now();

            Data.JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(jobHistoryId);
            jobHistory.JobStatus = JobStatusChoices.JobHistoryErrorJobFailed;
            jobHistory.EndTimeUTC = endTime;
            _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);

            JobHistoryError jobHistoryError = new JobHistoryError
            {
                ParentArtifactId = jobHistoryId,
                JobHistory = jobHistoryId,
                Name = Guid.NewGuid().ToString(),
                ErrorType = ErrorTypeChoices.JobHistoryErrorJob,
                ErrorStatus = ErrorStatusChoices.JobHistoryErrorNew,
                SourceUniqueID = string.Empty,
                Error = ex.Message,
                StackTrace = ex.FlattenErrorMessagesWithStackTrace(),
                TimestampUTC = endTime,
            };
            _objectManager.Create(jobHistoryError);

            _integrationPointRepository.UpdateHasErrors(integrationPointId, true);
            _integrationPointRepository.UpdateLastAndNextRunTime(integrationPointId, endTime, null);
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
                    $"User is missing the following permissions:{Environment.NewLine}{string.Join(Environment.NewLine, ex.Message)}");
                throw;
            }
        }

        private void StopSyncAppJobs(Data.JobHistory jobHistory)
        {
            try
            {
                _relativitySyncAppIntegration.CancelJobAsync(Guid.Parse(jobHistory.JobID)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel Sync Job ID: {jobId}", jobHistory.JobID);
            }
        }

        private void StopExecutingJob(int integrationPointArtifactId, Guid jobHistoryGuid)
        {
            IDictionary<Guid, List<Job>> jobs = _jobManager.GetJobsByJobHistoryGuid(integrationPointArtifactId);
            _logger.LogInformation("Jobs marked to stopping with correspondent JobHistoryGuid {@jobsByJobHistoryGuid}", jobs);

            if (jobs.ContainsKey(jobHistoryGuid))
            {
                IList<long> jobIdsForGivenJobHistory = jobs[jobHistoryGuid].Select(x => x.JobId).ToList();
                _jobManager.StopJobs(jobIdsForGivenJobHistory);
                _logger.LogInformation("Jobs {@jobs} has been marked to stop for {jobHistoryGuid}", jobIdsForGivenJobHistory, jobHistoryGuid);
            }
        }

        private void StopPendingJob(int integrationPointArtifactId, Data.JobHistory jobHistory, Guid jobHistoryGuid)
        {
            IDictionary<Guid, List<Job>> jobs = _jobManager.GetJobsByBatchInstanceId(integrationPointArtifactId);
            _logger.LogInformation("Jobs marked to stopping with correspondent BatchInstanceId {@jobs}", jobs);

            jobHistory.JobStatus = JobStatusChoices.JobHistoryStopped;
            _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);

            Guid batchInstance = Guid.Parse(jobHistory.BatchInstance);

            if (jobs.ContainsKey(batchInstance))
            {
                jobs[batchInstance].ForEach(x => _jobManager.DeleteJob(x.JobId));

                _logger.LogInformation("Jobs {@jobs} has been deleted from queue and JobHistory {jobHistoryId} was set to Stopped", jobs[batchInstance], jobHistory.ArtifactId);
            }
        }

        private Job CreateJob(
            IntegrationPointDto integrationPoint,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            string correlationID,
            int workspaceArtifactId,
            int userId)
        {
            _logger.LogInformation("Creating Job for Integration Point {integrationPointId} by user {userId}...", integrationPoint.ArtifactId, userId);

            Job job = null;
            lock (Lock)
            {
                // If the Relativity provider is selected, we need to create an export task
                TaskType jobTaskType = GetJobTaskType(sourceProvider, destinationProvider);

                TaskParameters jobDetails = _taskParametersBuilder.Build(jobTaskType, Guid.Parse(correlationID), integrationPoint.SourceConfiguration, integrationPoint.DestinationConfiguration);

                job = _jobManager.CreateJobOnBehalfOfAUser(jobDetails, jobTaskType, correlationID, workspaceArtifactId, integrationPoint.ArtifactId, userId);
            }

            _logger.LogInformation("Job was successfully created.");
            return job;
        }

        private Data.JobHistory CreateJobHistory(IntegrationPointDto integrationPointDto, Guid batchInstance, ChoiceRef jobType, bool switchToAppendOverlayMode = false)
        {
            _logger.LogInformation("Creating Job History for Integration Point {integrationPointId} with BatchInstance {batchInstance}...", integrationPointDto.ArtifactId, batchInstance);

            Data.JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPointDto, batchInstance, jobType, null);
            AdjustOverwriteModeForRetry(jobHistory, switchToAppendOverlayMode);

            if (jobHistory == null)
            {
                _logger.LogError(Constants.IntegrationPoints.FAILED_TO_CREATE_JOB_HISTORY);
                throw new Exception(Constants.IntegrationPoints.FAILED_TO_CREATE_JOB_HISTORY);
            }

            _logger.LogInformation("Job History {jobHistoryId} was created for Integration Point {integrationPointId}.", jobHistory.ArtifactId, integrationPointDto.ArtifactId);

            return jobHistory;
        }

        private void AdjustOverwriteModeForRetry(Data.JobHistory jobHistory, bool switchToAppendOverlayMode)
        {
            if (switchToAppendOverlayMode)
            {
                jobHistory.Overwrite = OverwriteFieldsChoices.IntegrationPointAppendOverlay.Name;
                _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
            }
        }

        private void SetJobHistoryStatus(Data.JobHistory jobHistory, ChoiceRef status)
        {
            if (jobHistory != null)
            {
                jobHistory.JobStatus = status;
                _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
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

        private void HandleValidationError(Data.JobHistory jobHistory, IntegrationPointDto integrationPointDto, Exception ex)
        {
            AddValidationErrorToJobHistory(jobHistory, ex);
            AddValidationErrorToErrorTab(ex);
            SetJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryValidationFailed);
            SendValidationFailedMessage(integrationPointDto, jobHistory.BatchInstance);
            _integrationPointRepository.UpdateHasErrors(integrationPointDto.ArtifactId, true);
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

        private void ValidateLastJobHistoryBeforeRun(
            int workspaceArtifactId,
            int integrationPointArtifactId)
        {
            _logger.LogInformation(
                "Checking if last Job History for Integration Point {integrationPointId} is valid for run",
                integrationPointArtifactId);

            IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager();
            ChoiceRef lastJobHistoryStatus = jobHistoryManager.GetLastJobHistoryStatus(workspaceArtifactId, integrationPointArtifactId);

            bool lastStatusAllowsRun = lastJobHistoryStatus.EqualsToAnyChoice(
                null,
                JobStatusChoices.JobHistoryCompleted,
                JobStatusChoices.JobHistoryCompletedWithErrors,
                JobStatusChoices.JobHistoryErrorJobFailed,
                JobStatusChoices.JobHistoryStopped,
                JobStatusChoices.JobHistoryValidationFailed);

            if (lastStatusAllowsRun == false)
            {
                string errorMessage = "Run is not allowed for current job status.";
                throw new IntegrationPointValidationException(new ValidationResult(false, errorMessage));
            }
        }

        private void ValidateLastJobHistoryBeforeRetry(
            int workspaceArtifactId,
            int integrationPointArtifactId,
            ProviderType providerType)
        {
            _logger.LogInformation(
                "Checking if provider type and last Job History for Integration Point {integrationPointId} is valid for retry",
                integrationPointArtifactId);

            IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager();
            ChoiceRef lastJobHistoryStatus = jobHistoryManager.GetLastJobHistoryStatus(workspaceArtifactId, integrationPointArtifactId);
            bool completedWithErrors = lastJobHistoryStatus.EqualsToChoice(JobStatusChoices.JobHistoryCompletedWithErrors);

            if (providerType != ProviderType.Relativity || completedWithErrors == false)
            {
                string errorMessage = "Retry is allowed only if provider is relativity and last run completed with errors.";
                throw new IntegrationPointValidationException(new ValidationResult(false, errorMessage));
            }
        }

        private IntegrationPointDto ToDto(Data.IntegrationPoint rdo)
        {
            // May have your attention please!
            // Long Text fields are intentionally not mapped here to avoid deserialization issues of truncated jsons
            return new IntegrationPointDto
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
                JobHistory = rdo.JobHistory?.ToList(),
            };
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

            var integrationPointRdo = new Data.IntegrationPoint
            {
                ArtifactId = dto.ArtifactId,
                Name = dto.Name,
                OverwriteFields = new ChoiceRef(choice.ArtifactID) { Name = choice.Name },
                SourceConfiguration = string.IsNullOrEmpty(dto.SourceConfiguration)
                    ? Serializer.Serialize(new Dictionary<string, object>())
                    : dto.SourceConfiguration,
                SourceProvider = dto.SourceProvider,
                Type = dto.Type,
                DestinationConfiguration = Serializer.Serialize(dto.DestinationConfiguration ?? new DestinationConfiguration()),
                FieldMappings = Serializer.Serialize(dto.FieldMappings ?? new List<FieldMap>()),
                EnableScheduler = dto.Scheduler.EnableScheduler,
                DestinationProvider = dto.DestinationProvider,
                LogErrors = dto.LogErrors,
                HasErrors = dto.HasErrors,
                EmailNotificationRecipients =
                    string.Join("; ", (dto.EmailNotificationRecipients ?? string.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList()),
                LastRuntimeUTC = dto.LastRun,
                SecuredConfiguration = dto.SecuredConfiguration,
                JobHistory = dto.JobHistory?.ToArray(),
            };

            if (integrationPointRdo.EnableScheduler.GetValueOrDefault(false))
            {
                integrationPointRdo.ScheduleRule = rule.ToSerializedString();
                integrationPointRdo.NextScheduledRuntimeUTC = rule.GetNextUTCRunDateTime();
            }
            else
            {
                integrationPointRdo.ScheduleRule = string.Empty;
                integrationPointRdo.NextScheduledRuntimeUTC = null;
            }

            return integrationPointRdo;
        }

        private bool FilterSyncAppJobHistory(Data.JobHistory jobHistory)
        {
            // When Job ID is a valid GUID then we know it's a Sync App Job
            return Guid.TryParse(jobHistory.JobID, out Guid jobId);
        }
    }
}
