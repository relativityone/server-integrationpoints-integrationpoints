using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;
using Relativity.Services.Choice;
using Relativity.Telemetry.APM;
using Client = Relativity.Telemetry.APM.Client;

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

			if (updatedJob.StopState == StopState.Stopping)
			{
				return;
			}

			JobHistory jobHistory = GetHistory(job);
			ChoiceRef newStatus = JobStatusChoices.JobHistoryProcessing;
			string oldStatusName = jobHistory.JobStatus.Name;

			jobHistory.JobStatus = newStatus;

			UpdateJobHistory(jobHistory,
				oldStatusName,
				newStatus.Name,
				job.JobId,
				jobHistory.ArtifactId);
		}

		public void OnJobComplete(Job job)
		{
			JobHistory jobHistory = GetHistory(job);

			ChoiceRef newStatus = _updater.GenerateStatus(jobHistory, job.JobId);
			string oldStatusName = jobHistory.JobStatus.Name;

			jobHistory.JobStatus = newStatus;
			jobHistory.EndTimeUTC = _dateTimeHelper.Now();
			SendHealthCheck(jobHistory, job.WorkspaceID);

			UpdateJobHistory(jobHistory,
				oldStatusName,
				newStatus.Name,
				job.JobId,
				jobHistory.ArtifactId);
		}

		private JobHistory GetHistory(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(
				taskParameters.BatchInstance
			);

			if (jobHistory == null)
			{
				ThrowWhenJobHistoryNotRetrieved(job);
			}

			return jobHistory;
		}

		private void UpdateJobHistory(JobHistory jobHistory, 
			string oldStatusName, 
			string newStatusName, 
			long jobId, 
			int jobHistoryArtifactId)
		{
			
			try
			{
				_jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception,
					_JOB_UPDATE_ERROR_MESSAGE_TEMPLATE,
					oldStatusName,
					newStatusName,
					jobId,
					jobHistoryArtifactId);
				throw;
			}
		}
		
		private void SendHealthCheck(JobHistory jobHistory, long workspaceID)
		{
			if (!IsJobFailed(jobHistory.JobStatus))
			{
				return;
			}
			IHealthMeasure healthCheck = Client.APMClient.HealthCheckOperation(
				Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK,
				() => HealthCheck.CreateJobFailedMetric(jobHistory, workspaceID));
			healthCheck.Write();
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