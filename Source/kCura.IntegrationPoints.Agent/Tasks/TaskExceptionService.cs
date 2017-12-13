using System;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class TaskExceptionService : ITaskExceptionService
	{
		private readonly IJobHistoryErrorService _jobHistoryErrorService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobService _jobService;

		public TaskExceptionService(IJobHistoryErrorService jobHistoryErrorService, IJobHistoryService jobHistoryService, IJobService jobService)
		{
			_jobHistoryErrorService = jobHistoryErrorService;
			_jobHistoryService = jobHistoryService;
			_jobService = jobService;
		}

		public void EndTaskWithError(ITask task, Exception ex)
		{
			var taskWithJobHistory = task as ITaskWithJobHistory;

			if (taskWithJobHistory != null)
			{
				_jobHistoryErrorService.JobHistory = taskWithJobHistory.JobHistory;
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, ex.Message, ex.StackTrace);
				taskWithJobHistory.JobHistory.JobStatus = JobStatusChoices.JobHistoryErrorJobFailed;
				_jobHistoryService.UpdateRdo(taskWithJobHistory.JobHistory);
				_jobService.CleanupJobQueueTable();
			}
		}
	}
}
