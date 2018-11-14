using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class JobHistoryBatchUpdateStatus : IBatchStatus
	{
		private const string _JOB_HISTORY_NULL = "Failed to retrieve job history. job ID: {0}.";
		private const string _JOB_UPDATE_ERROR_MESSAGE_TEMPLATE = "Failed to update finished job error status. Current status: {0}, target status: {1}, job ID: {2}, job history artifact ID: {3}.";

		private readonly IJobStatusUpdater _updater;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobService _jobService;
		private readonly ISerializer _serializer;
		private readonly IAPILog _logger;

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
			JobHistory result = GetHistory(job);
			if (result == null)
			{
				long jobId = job?.JobId ?? -1;
				string message = string.Format(_JOB_HISTORY_NULL, jobId);
				NullReferenceException exception = new NullReferenceException(message);
				_logger.LogError(exception, _JOB_HISTORY_NULL, jobId);
				throw exception;
			}
			int artifactId = result.ArtifactId;
			string oldStatusName = result.JobStatus.Name;
			result.JobStatus = _updater.GenerateStatus(result, job.WorkspaceID);
			string newStatusName = result.JobStatus.Name;
			result.EndTimeUTC = DateTime.UtcNow;
			try
			{
				_jobHistoryService.UpdateRdo(result);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, _JOB_UPDATE_ERROR_MESSAGE_TEMPLATE, oldStatusName, newStatusName, job.JobId, artifactId);
				throw;
			}
		}

		private JobHistory GetHistory(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			return _jobHistoryService.GetRdo(taskParameters.BatchInstance);
		}
	}
}