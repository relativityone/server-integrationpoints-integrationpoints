using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.QueryOptions;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class JobHistoryBatchUpdateStatus : IBatchStatus
	{
		private const string _JOB_HISTORY_NULL = "Failed to retrieve job history. job ID: {0}.";
		private const string _JOB_UPDATE_ERROR_MESSAGE_TEMPLATE = "Failed to update job status. Current status: {0}, target status: {1}, job ID: {2}, job history artifact ID: {3}.";

		private readonly IJobStatusUpdater _updater;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobService _jobService;
		private readonly ISerializer _serializer;
		private readonly IAPILog _logger;
		private readonly IDateTimeHelper _dateTimeHelper;

		private readonly JobHistoryQueryOptions _jobHistoryQueryOptions =
			JobHistoryQueryOptions.Query
				.All()
				.Except(JobHistoryFieldGuids.Documents);

	    public JobHistoryBatchUpdateStatus(IJobStatusUpdater jobStatusUpdater, 
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
			Choice newStatus = JobStatusChoices.JobHistoryProcessing;
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
			
			Choice newStatus = _updater.GenerateStatus(jobHistory, job.WorkspaceID);
			string oldStatusName = jobHistory.JobStatus.Name;

			jobHistory.JobStatus = newStatus;
			jobHistory.EndTimeUTC = _dateTimeHelper.Now();

			UpdateJobHistory(jobHistory,
				oldStatusName,
				newStatus.Name,
				job.JobId,
				jobHistory.ArtifactId);
		}

		private JobHistory GetHistory(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			JobHistory jobHistory = _jobHistoryService.GetRdo(
				taskParameters.BatchInstance, 
				_jobHistoryQueryOptions
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
				_jobHistoryService.UpdateRdo(
					jobHistory,
					_jobHistoryQueryOptions
				);
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