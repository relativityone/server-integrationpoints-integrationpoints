using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks.Helpers
{
	public class TaskCleanupHelper
	{
		private readonly IJobHistoryErrorService _jobHistoryErrorService;
		private readonly JobHistory _jobHistory;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobService _jobService;

		public TaskCleanupHelper(IJobHistoryErrorService jobHistoryErrorService, JobHistory jobHistory, IJobHistoryService jobHistoryService, IJobService jobService)
		{
			_jobHistoryErrorService = jobHistoryErrorService;
			_jobHistory = jobHistory;
			_jobHistoryService = jobHistoryService;
			_jobService = jobService;
		}

		public void EndTaskWithError(Exception ex)
		{
			_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, ex.Message, ex.StackTrace);
			_jobHistory.JobStatus = JobStatusChoices.JobHistoryErrorJobFailed;
			_jobHistoryService.UpdateRdo(_jobHistory);
			_jobService.CleanupJobQueueTable();
		}
	}
}
