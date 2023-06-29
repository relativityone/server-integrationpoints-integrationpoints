using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.Sync;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Common.RelativitySync;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    public class TaskFactory : ITaskFactory
    {
        private readonly IWindsorContainer _container;
        private readonly ITaskExceptionMediator _taskExceptionMediator;
        private readonly IAPILog _logger;
        private readonly IJobSynchronizationChecker _jobSynchronizationChecker;
        private readonly ITaskFactoryJobHistoryServiceFactory _jobHistoryServiceFactory;
        private readonly IIntegrationPointService _integrationPointService;

        public TaskFactory(
            IAgentHelper helper,
            ITaskExceptionMediator taskExceptionMediator,
            IJobSynchronizationChecker jobSynchronizationChecker,
            ITaskFactoryJobHistoryServiceFactory jobHistoryServiceFactory,
            IWindsorContainer container,
            IIntegrationPointService integrationPointService)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<TaskFactory>();
            _taskExceptionMediator = taskExceptionMediator;
            _jobSynchronizationChecker = jobSynchronizationChecker;
            _jobHistoryServiceFactory = jobHistoryServiceFactory;
            _container = container;
            _integrationPointService = integrationPointService;
        }

        public ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase)
        {
            LogCreatingTaskInformation(job);
            _taskExceptionMediator.RegisterEvent(agentBase);

            if (_container == null)
            {
                LogContainerNotInitialized(job);
                throw new InvalidOperationException($"{nameof(TaskFactory)} wasn't properly initialized. Container cannot be null.");
            }

            IntegrationPointDto integrationPointDto = GetIntegrationPoint(job);
            ITaskFactoryJobHistoryService jobHistoryServices = _jobHistoryServiceFactory.CreateJobHistoryService(integrationPointDto);
            try
            {
                jobHistoryServices.SetJobIdOnJobHistory(job);
                TaskType taskType;
                Enum.TryParse(job.TaskType, true, out taskType);
                LogCreateTaskSyncCheck(job, taskType);

                switch (taskType)
                {
                    case TaskType.SyncManager:
                        if (NewCustomProviderFlowShouldBeUsed(integrationPointDto))
                        {
                            return _container.Resolve<ICustomProviderTask>();
                        }

                        return CheckForSynchronizationAndResolve<SyncManager>(job, integrationPointDto, agentBase);
                    case TaskType.SyncWorker:
                        return CheckForSynchronizationAndResolve<SyncWorker>(job, integrationPointDto, agentBase);
                    case TaskType.SyncEntityManagerWorker:
                        return CheckForSynchronizationAndResolve<SyncEntityManagerWorker>(job, integrationPointDto, agentBase);
                    case TaskType.SendEmailManager: // Left for backwards compatibility after removing SendEmailManager
                    case TaskType.SendEmailWorker:
                        return CheckForSynchronizationAndResolve<SendEmailWorker>(job, integrationPointDto, agentBase);
                    case TaskType.ExportService:
                        if (RelativitySyncAppShouldBeUsed(integrationPointDto))
                        {
                            return _container.Resolve<IScheduledSyncTask>();
                        }

                        return CheckForSynchronizationAndResolve<ExportServiceManager>(job, integrationPointDto, agentBase);
                    case TaskType.ImportService:
                        return CheckForSynchronizationAndResolve<ImportServiceManager>(job, integrationPointDto, agentBase);
                    case TaskType.ExportManager:
                        return CheckForSynchronizationAndResolve<ExportManager>(job, integrationPointDto, agentBase);
                    case TaskType.ExportWorker:
                        return CheckForSynchronizationAndResolve<ExportWorker>(job, integrationPointDto, agentBase);
                    default:
                        throw UnknownTaskTypeException(taskType);
                }
            }
            catch (AgentDropJobException e)
            {
                // We catch this type of exception when an agent explicitly drops a Job, and we bubble up the exception message to the Errors tab.
                LogAgentDropJobException(job, e);
                throw;
            }
            catch (Exception e)
            {
                LogErrorDuringTaskCreation(e, job.TaskType, job.JobId);
                jobHistoryServices.UpdateJobHistoryOnFailure(job, e);
                throw;
            }
        }

        private bool NewCustomProviderFlowShouldBeUsed(IntegrationPointDto integrationPointDto)
        {
            ICustomProviderFlowCheck newFlowCheck = _container.Resolve<ICustomProviderFlowCheck>();
            bool useNewFlow = newFlowCheck.ShouldBeUsed(integrationPointDto);

            _logger.LogInformation("Using new flow to execute Custom Provider import: {useNewFlow}", useNewFlow);

            return useNewFlow;
        }

        private bool RelativitySyncAppShouldBeUsed(IntegrationPointDto integrationPointDto)
        {
            return _container.Resolve<IRelativitySyncConstrainsChecker>().ShouldUseRelativitySyncApp(integrationPointDto.ArtifactId);
        }

        private ITask CheckForSynchronizationAndResolve<T>(Job job, IntegrationPointDto integrationPointDto, ScheduleQueueAgentBase agentBase) where T : ITask
        {
            _jobSynchronizationChecker.CheckForSynchronization(typeof(T), job, integrationPointDto, agentBase);
            return _container.Resolve<T>();
        }

        private IntegrationPointDto GetIntegrationPoint(Job job)
        {
            LogGetIntegrationPointStart(job);
            IntegrationPointDto integrationPoint = _integrationPointService.Read(job.RelatedObjectArtifactID);

            if (integrationPoint == null)
            {
                LogIntegrationPointNotFound(job);
                throw new NullReferenceException(
                    $"Unable to retrieve the integration point for the following job: {job.JobId}");
            }

            LogGetIntegrationPointSuccesfullEnd(job, integrationPoint);
            return integrationPoint;
        }

        private IntegrationPointsException UnknownTaskTypeException(TaskType taskType)
        {
            _logger.LogError("Unable to create task. Unknown task type ({TaskType})", taskType);

            return new IntegrationPointsException($"Unable to create task. Unknown task type ({taskType})");
        }

        #region Logging

        private void LogContainerNotInitialized(Job job)
        {
            _logger.LogFatal("{object} not properly initialized for {JobId}.", nameof(TaskFactory), job.JobId);
        }

        private void LogCreatingTaskInformation(Job job)
        {
            _logger.LogInformation("Attempting to create task {TaskType} for job {JobId}.", job.TaskType, job.JobId);
        }

        private void LogCreateTaskSyncCheck(Job job, TaskType taskType)
        {
            _logger.LogInformation("Creating job specific manger/worker class in task factory. Job: {JobId}, Task Type: {TaskType}", job.JobId, taskType);
        }

        private void LogErrorDuringTaskCreation(Exception exception, string taskType, long jobId)
        {
            _logger.LogError(exception, "Error during task creation ({TaskType}) for job {JobId}", taskType, jobId);
        }

        private void LogAgentDropJobException(Job job, AgentDropJobException agentDropJobException)
        {
            _logger.LogError(agentDropJobException, "Agent explicitly dropped job {JobId}.", job.JobId);
        }

        private void LogIntegrationPointNotFound(Job job)
        {
            _logger.LogError("Unable to retrieve the integration point for the following job: {JobId}", job.JobId);
        }

        private void LogGetIntegrationPointSuccesfullEnd(Job job, IntegrationPointDto integrationPoint)
        {
            _logger.LogInformation("Read integration point record completed for job: {JobId}, IP ArtifactId: {ArtifactId}", job.JobId, integrationPoint.ArtifactId);
        }

        private void LogGetIntegrationPointStart(Job job)
        {
            _logger.LogInformation("Reading integration point record for job: {JobId}", job.JobId);
        }
        #endregion
    }
}
