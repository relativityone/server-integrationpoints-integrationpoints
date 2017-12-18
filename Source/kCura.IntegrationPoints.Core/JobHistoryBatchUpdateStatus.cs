using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.Core
{
	public class JobHistoryBatchUpdateStatus : IBatchStatus
	{
		private readonly IJobStatusUpdater _updater;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobService _jobService;
		private readonly ISerializer _serializer;

		public JobHistoryBatchUpdateStatus(IJobStatusUpdater jobStatusUpdater, IJobHistoryService jobHistoryService, IJobService jobService, ISerializer serializer)
		{
			_updater = jobStatusUpdater;
			_jobHistoryService = jobHistoryService;
			_jobService = jobService;
			_serializer = serializer;
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
			result.JobStatus = _updater.GenerateStatus(result, job.JobId);
			result.EndTimeUTC = DateTime.UtcNow;
			_jobHistoryService.UpdateRdo(result);
		}

		private JobHistory GetHistory(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			return _jobHistoryService.GetRdo(taskParameters.BatchInstance);
		}
	}
}