﻿using System;
using System.Globalization;

using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Interfaces;

using Relativity.API;
using Relativity.Services.Choice;

using OtelSdk = Relativity.OpenTelemetry.OtelSdk;

namespace kCura.IntegrationPoints.Core
{
    public class JobHistoryBatchUpdateStatus : IBatchStatus
    {
        private const string _JOB_HISTORY_NULL = "Failed to retrieve job history. job ID: {0}.";
        private const string _JOB_UPDATE_ERROR_MESSAGE_TEMPLATE = "Failed to update job status. Current status: {0}, target status: {1}, job ID: {2}, job history artifact ID: {3}.";
        private readonly IJobStatusUpdater _updater;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobService _jobService;
        private readonly IAPILog _logger;
        private readonly ISerializer _serializer;
        private readonly IDateTimeHelper _dateTimeHelper;

        public JobHistoryBatchUpdateStatus(
            IJobStatusUpdater jobStatusUpdater,
            IJobHistoryService jobHistoryService,
            IJobService jobService,
            ISerializer serializer,
            IAPILog logger,
            IDateTimeHelper dateTimeHelper)
        {
            _updater = jobStatusUpdater;
            _jobHistoryService = jobHistoryService;
            _jobService = jobService;
            _serializer = serializer;
            _logger = logger;
            _dateTimeHelper = dateTimeHelper;
        }

        public void OnJobStart(Job job)
        {
            Job updatedJob = _jobService.GetJob(job.JobId);

            if (updatedJob == null)
            {
                throw new InvalidOperationException($"Cannot find job with ID: {job.JobId}");
            }

            if (updatedJob.StopState == StopState.Stopping)
            {
                return;
            }

            JobHistory jobHistory = GetHistory(job);
            UpdateJobHistory(jobHistory, JobStatusChoices.JobHistoryProcessing, job.JobId);
        }

        public void OnJobComplete(Job job)
        {
            JobHistory jobHistory = GetHistory(job);
            jobHistory.EndTimeUTC = _dateTimeHelper.Now();

            ChoiceRef newStatus = _updater.GenerateStatus(jobHistory, job.JobId);
            SendHealthCheck(job.JobId, job.WorkspaceID, !IsJobFailed(newStatus));
            UpdateJobHistory(jobHistory, newStatus, job.JobId);
        }

        private JobHistory GetHistory(Job job)
        {
            TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
            JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(taskParameters.BatchInstance);

            if (jobHistory == null)
            {
                ThrowWhenJobHistoryNotRetrieved(job);
            }

            return jobHistory;
        }

        private void UpdateJobHistory(JobHistory jobHistory, ChoiceRef newStatus, long jobId)
        {
            string oldStatusName = null;
            try
            {
                oldStatusName = jobHistory.JobStatus?.Name;
                jobHistory.JobStatus = newStatus;
                _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    _JOB_UPDATE_ERROR_MESSAGE_TEMPLATE,
                    oldStatusName,
                    jobHistory.JobStatus?.Name,
                    jobId,
                    jobHistory.ArtifactId);
                throw;
            }
        }

        private void SendHealthCheck(long jobId, long workspaceId, bool isHealthy)
        {
            OtelSdk.Instance.RecordHealthCheck(
                Constants.IntegrationPoints.OpenTelemetry.HEALTH_CHECK_JOB_HISTORY_NAME,
                Constants.IntegrationPoints.OpenTelemetry.HEALTH_CHECK_AGENT_EVENT_SOURCE,
                string.Format(CultureInfo.CurrentCulture,
                    isHealthy
                        ? Constants.IntegrationPoints.OpenTelemetry.HEALTH_CHECK_JOB_HISTORY_SUCCESSFUL_MESSAGE
                        : Constants.IntegrationPoints.OpenTelemetry.HEALTH_CHECK_JOB_HISTORY_FAILED_MESSAGE, jobId,
                    workspaceId),
                isHealthy: isHealthy,
                workspaceId: Convert.ToInt32(workspaceId));
        }

        private bool IsJobFailed(ChoiceRef jobStatusChoice)
        {
            return jobStatusChoice.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed) || jobStatusChoice.EqualsToChoice(JobStatusChoices.JobHistoryErrorJobFailed);
        }

        private void ThrowWhenJobHistoryNotRetrieved(Job job)
        {
            long jobId = job?.JobId ?? -1;
            string message = string.Format(_JOB_HISTORY_NULL, jobId);
            var exception = new NullReferenceException(message);
            _logger.LogError(exception, _JOB_HISTORY_NULL, jobId);
            throw exception;
        }
    }
}
