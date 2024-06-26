﻿using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
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
        private readonly ISerializer _serializer;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IntegrationPointDto _integrationPoint;

        public TaskFactoryJobHistoryService(
            IAPILog logger,
            ISerializer serializer,
            IJobHistoryErrorService jobHistoryErrorService,
            IIntegrationPointService integrationPointService,
            IJobHistoryService jobHistoryService,
            IntegrationPointDto integrationPoint)
        {
            _logger = logger.ForContext<TaskFactoryJobHistoryService>();
            _serializer = serializer;
            _jobHistoryErrorService = jobHistoryErrorService;
            _integrationPointService = integrationPointService;
            _jobHistoryService = jobHistoryService;
            _integrationPoint = integrationPoint;
        }

        public void SetJobIdOnJobHistory(Job job)
        {
            JobHistory jobHistory = GetJobHistory(job);
            if (jobHistory != null && string.IsNullOrEmpty(jobHistory.JobID))
            {
                jobHistory.JobID = job.JobId.ToString();
                _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
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

            _integrationPoint.JobHistory.Remove(jobHistory.ArtifactId);
            _integrationPointService.UpdateJobHistory(_integrationPoint.ArtifactId, _integrationPoint.JobHistory);

            jobHistory.JobStatus = JobStatusChoices.JobHistoryStopped;
            _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
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
            JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(taskParameters.BatchInstance);

            LogGetJobHistorySuccessfulEnd(job, jobHistory);
            return jobHistory;
        }

        private void UpdateJobHistoryOnError(Job job, ChoiceRef jobHistoryStatus, Exception e)
        {
            LogUpdateJobHistoryOnFailureStart(job, e);
            JobHistory jobHistory = GetJobHistory(job);

            _jobHistoryErrorService.IntegrationPointDto = _integrationPoint;
            _jobHistoryErrorService.JobHistory = jobHistory;
            _jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
            _jobHistoryErrorService.CommitErrors();
            jobHistory.JobStatus = jobHistoryStatus;
            jobHistory.EndTimeUTC = DateTime.UtcNow;
            _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);

            // No updates to IP since the job history error service handles IP updates
            IHealthMeasure healthcheck = Client.APMClient.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => HealthCheck.CreateJobFailedMetric(jobHistory.JobID, job.WorkspaceID));
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
