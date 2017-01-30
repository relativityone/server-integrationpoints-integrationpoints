using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Core.Contracts.Helpers;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class CorrespondingJobDelete : ICorrespondingJobDelete
	{
		private readonly IJobService _jobService;

		public CorrespondingJobDelete(IJobService jobService)
		{
			_jobService = jobService;
		}

		public void DeleteCorrespondingJob(int workspaceId, int integrationPointArtifactId)
		{
			var taskTypes = TaskTypeHelper.GetManagerTypes().Select(taskType => taskType.ToString()).ToList();
			IEnumerable<Job> jobs = _jobService.GetScheduledJobs(workspaceId, integrationPointArtifactId, taskTypes);
			jobs.ForEach(job => _jobService.DeleteJob(job.JobId));
		}
	}
}