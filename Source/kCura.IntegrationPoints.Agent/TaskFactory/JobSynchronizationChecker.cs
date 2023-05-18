using System;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    internal class JobSynchronizationChecker : IJobSynchronizationChecker
    {
        private readonly IAPILog _logger;
        private readonly IJobService _jobService;
        private readonly IManagerFactory _managerFactory;
        private readonly ITaskFactoryJobHistoryServiceFactory _jobHistoryServicesFactory;

        public JobSynchronizationChecker(
            IAgentHelper helper,
            IJobService jobService,
            IManagerFactory managerFactory,
            ITaskFactoryJobHistoryServiceFactory jobHistoryServicesFactory)
        {
            _jobService = jobService;
            _managerFactory = managerFactory;
            _jobHistoryServicesFactory = jobHistoryServicesFactory;

            _logger = helper.GetLoggerFactory().GetLogger().ForContext<JobSynchronizationChecker>();
        }

        public void CheckForSynchronization(Type type, Job job, IntegrationPointDto integrationPointDto, ScheduleQueueAgentBase agentBase)
        {
            object[] attributes = type.GetCustomAttributes(false);
            foreach (object attribute in attributes)
            {
                if (attribute is SynchronizedTaskAttribute)
                {
                    if (HasOtherJobsExecuting(job))
                    {
                        DropJobAndThrowException(job, integrationPointDto, agentBase);
                    }
                    break;
                }
            }
        }

        internal bool HasOtherJobsExecuting(Job job)
        {
            IQueueManager queueManager = _managerFactory.CreateQueueManager();

            bool hasOtherJobsExecuting = queueManager.HasJobsExecuting(job.WorkspaceID, job.RelatedObjectArtifactID, job.JobId, job.NextRunTime);

            return hasOtherJobsExecuting;
        }

        internal void DropJobAndThrowException(Job job, IntegrationPointDto integrationPointDto, ScheduleQueueAgentBase agentBase)
        {
            string exceptionMessage = "Unable to execute Integration Point job: There is already a job currently running.";

            // check if it's a scheduled job
            if (!string.IsNullOrEmpty(job.ScheduleRuleType))
            {
                integrationPointDto.NextRun = _jobService.GetJobNextUtcRunDateTime(job, agentBase.ScheduleRuleFactory, new TaskResult { Status = TaskStatusEnum.None });
                exceptionMessage = $@"{exceptionMessage} Job is re-scheduled for {integrationPointDto.NextRun}.";
            }
            else
            {
                ITaskFactoryJobHistoryService jobHistoryService = _jobHistoryServicesFactory.CreateJobHistoryService(integrationPointDto);
                jobHistoryService.RemoveJobHistoryFromIntegrationPoint(job);
            }

            LogDroppingJob(job, integrationPointDto, exceptionMessage);

            throw new AgentDropJobException(exceptionMessage);
        }

        private void LogDroppingJob(Job job, IntegrationPointDto integrationPointDto, string exceptionMessage)
        {
            _logger.LogError("{ExceptionMessage}. Job Id: {JobId}. Task type: {TaskType}. Integration Point Id: {IntegrationPointId}.", exceptionMessage, job.JobId, job.TaskType,
                integrationPointDto.ArtifactId);
        }
    }
}
