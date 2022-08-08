using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;
using Relativity.Services.Choice;
using Relativity.Telemetry.APM;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    internal class TaskFactoryJobHistoryService : ITaskFactoryJobHistoryService
    {
        private readonly IAPILog _logger;
        private readonly IIntegrationPointSerializer _serializer;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IIntegrationPointRepository _integrationPointRepository;

        private readonly IntegrationPoint _integrationPoint;

        public TaskFactoryJobHistoryService(
            IAPILog logger, 
            IIntegrationPointSerializer serializer, 
            IServiceFactory serviceFactory, 
            IJobHistoryErrorService jobHistoryErrorService,
            IIntegrationPointRepository integrationPointRepository,
            IntegrationPoint integrationPoint)
        {
            _logger = logger.ForContext<TaskFactoryJobHistoryService>();
            _serializer = serializer;
            _jobHistoryErrorService = jobHistoryErrorService;
            _integrationPointRepository = integrationPointRepository;

            _integrationPoint = integrationPoint;
            _jobHistoryService = serviceFactory.CreateJobHistoryService(_logger);
        }

        public void SetJobIdOnJobHistory(Job job)
        {
            JobHistory jobHistory = GetJobHistory(job);
            if (jobHistory != null && string.IsNullOrEmpty(jobHistory.JobID))
            {
                jobHistory.JobID = job.JobId.ToString();
                _jobHistoryService.UpdateRdo(jobHistory);
            }
        }

        public void UpdateJobHistoryOnFailure(Job job, Exception e)
        {
            UpdateJobHistoryOnError(job, JobStatusChoices.JobHistoryErrorJobFailed, e);
        }

        public void RemoveJobHistoryFromIntegrationPoint(Job job)
        {
            LogRemoveJobHistoryFromIntegrationPointStart();

            JobHistory jobHistory = GetJobHistory(job);
            if (jobHistory == null)
            {
                return;
            }

            List<int> jobHistoryIds = _integrationPoint.JobHistory.ToList();
            jobHistoryIds.Remove(jobHistory.ArtifactId);
            _integrationPoint.JobHistory = jobHistoryIds.ToArray();
            _integrationPointRepository.Update(_integrationPoint);

            jobHistory.JobStatus = JobStatusChoices.JobHistoryStopped;
            _jobHistoryService.UpdateRdo(jobHistory);
            _jobHistoryService.DeleteRdo(jobHistory.ArtifactId);

            LogRemoveJobHistoryFromIntegrationPointSuccessfulEnd();
        }

        private JobHistory GetJobHistory(Job job)
        {
            if (string.IsNullOrEmpty(job?.JobDetails))
            {
                return null;
            }
            LogGetJobHistoryStart(job);
            TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
            JobHistory jobHistory = _jobHistoryService.CreateRdo(
                _integrationPoint,
                taskParameters.BatchInstance,
                string.IsNullOrEmpty(job.ScheduleRuleType)
                    ? JobTypeChoices.JobHistoryRun
                    : JobTypeChoices.JobHistoryScheduledRun, DateTime.Now);

            LogGetJobHistorySuccessfulEnd(job, jobHistory);
            return jobHistory;
        }

        private void UpdateJobHistoryOnError(Job job, ChoiceRef jobHistoryStatus , Exception e)
        {
            LogUpdateJobHistoryOnFailureStart(job, e);
            JobHistory jobHistory = GetJobHistory(job);

            _jobHistoryErrorService.IntegrationPoint = _integrationPoint;
            _jobHistoryErrorService.JobHistory = jobHistory;
            _jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
            _jobHistoryErrorService.CommitErrors();
            jobHistory.JobStatus = jobHistoryStatus;
            _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);

            // No updates to IP since the job history error service handles IP updates
            IHealthMeasure healthcheck = Client.APMClient.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => HealthCheck.CreateJobFailedMetric(jobHistory, job.WorkspaceID));
            healthcheck.Write();
            LogUpdateJobHistoryOnFailureSuccesfulEnd(job);
        }

        #region logging

        private void LogRemoveJobHistoryFromIntegrationPointSuccessfulEnd()
        {
            _logger.LogInformation("Successfully removed job history from integration point: {ArtifactId}",
                _integrationPoint.ArtifactId);
        }

        private void LogRemoveJobHistoryFromIntegrationPointStart()
        {
            _logger.LogInformation("Removing job history from integration point: {ArtifactId}", _integrationPoint.ArtifactId);
        }

        private void LogGetJobHistorySuccessfulEnd(Job job, JobHistory jobHistory)
        {
            _logger.LogInformation("Successfully retrieved job history,  job: {JobId}, ArtifactId: {ArtifactId}, JobHistoryDetails: {jobHistoryDetails}", job.JobId,
                _integrationPoint.ArtifactId, jobHistory?.Stringify());
        }

        private void LogGetJobHistoryStart(Job job)
        {
            _logger.LogInformation("Getting job history,  job: {JobId}, ArtifactId: {ArtifactId} ", job.JobId,
                _integrationPoint.ArtifactId);
        }

        private void LogUpdateJobHistoryOnFailureSuccesfulEnd(Job job)
        {
            _logger.LogInformation("Successfully updated job history on failure,  job: {Job}, ArtifactId: {ArtifactId} ", job, _integrationPoint.ArtifactId);
        }

        private void LogUpdateJobHistoryOnFailureStart(Job job, Exception e)
        {
            _logger.LogInformation(e, "Updating job history on failure,  job: {Job}, ArtifactId: {ArtifactId} ", job, _integrationPoint.ArtifactId);
        }
        #endregion
    }
}
