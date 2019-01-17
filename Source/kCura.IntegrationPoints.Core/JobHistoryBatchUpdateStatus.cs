using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;
using Relativity.Telemetry.APM;
using Client = Relativity.Telemetry.APM.Client;

namespace kCura.IntegrationPoints.Core
{
	public class JobHistoryBatchUpdateStatus : IBatchStatus
	{
		private const string _JOB_HISTORY_NULL = "Failed to retrieve job history. job ID: {0}.";
		private const string _JOB_UPDATE_ERROR_MESSAGE_TEMPLATE = "Failed to update finished job error status. Current status: {0}, target status: {1}, job ID: {2}, job history artifact ID: {3}.";
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobService _jobService;
		private readonly IAPILog _logger;
		private readonly ISerializer _serializer;

		private readonly IJobStatusUpdater _updater;

		public JobHistoryBatchUpdateStatus(IJobStatusUpdater jobStatusUpdater, IJobHistoryService jobHistoryService,
			IJobService jobService, ISerializer serializer, IAPILog logger)
		{
			_updater = jobStatusUpdater;
			_jobHistoryService = jobHistoryService;
			_jobService = jobService;
			_serializer = serializer;
			_logger = logger;
		}

		public void OnJobStart(Job job)
		{
			Job updatedJob = _jobService.GetJob(job.JobId);
			if (updatedJob.StopState != StopState.Stopping)
			{
				JobHistory result = GetHistory(job);
				result.JobStatus = JobStatusChoices.JobHistoryProcessing;
				_jobHistoryService.UpdateRdo(result);
			}
		}

		public void OnJobComplete(Job job)
		{
			JobHistory jobHistory = GetHistory(job);
			if (jobHistory == null)
			{
				long jobId = job?.JobId ?? -1;
				string message = string.Format(_JOB_HISTORY_NULL, jobId);
				NullReferenceException exception = new NullReferenceException(message);
				_logger.LogError(exception, _JOB_HISTORY_NULL, jobId);
				throw exception;
			}

			int artifactId = jobHistory.ArtifactId;
			string oldStatusName = jobHistory.JobStatus.Name;
			jobHistory.JobStatus = _updater.GenerateStatus(jobHistory);
			SendHealthCheck(jobHistory, job.WorkspaceID);
			string newStatusName = jobHistory.JobStatus.Name;
			jobHistory.EndTimeUTC = DateTime.UtcNow;
			try
			{
				_jobHistoryService.UpdateRdo(jobHistory);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, _JOB_UPDATE_ERROR_MESSAGE_TEMPLATE, oldStatusName, newStatusName, job.JobId, artifactId);
				throw;
			}
		}

		private void SendHealthCheck(Data.JobHistory jobHistory, long workspaceID)
		{
			if (IsJobFailed(jobHistory.JobStatus))
			{
				IHealthMeasure healthcheck = Client.APMClient.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK,
					() => HealthCheck.CreateJobFailedMetric(jobHistory, workspaceID));
				healthcheck.Write();
			}
		}

		private bool IsJobFailed(Choice jobStatusChoice)
		{
			return jobStatusChoice.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed) || jobStatusChoice.EqualsToChoice(JobStatusChoices.JobHistoryErrorJobFailed);
		}

		private JobHistory GetHistory(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			return _jobHistoryService.GetRdo(taskParameters.BatchInstance);
		}
	}
}