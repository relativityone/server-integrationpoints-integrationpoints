using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.Management.Tasks.Helpers
{
	public class JobsWithInvalidStatus : IJobsWithInvalidStatus
	{
		private readonly IUnfinishedJobService _unfinishedJobService;
		private readonly IIntegrationPointSerializer _serializer;
		private readonly IJobService _jobService;

		public JobsWithInvalidStatus(IUnfinishedJobService unfinishedJobService, IIntegrationPointSerializer serializer, IJobService jobService)
		{
			_unfinishedJobService = unfinishedJobService;
			_serializer = serializer;
			_jobService = jobService;
		}

		public IDictionary<int, IList<JobHistory>> Find(IList<int> workspaceArtifactIds)
		{
			var invalidJobs = new Dictionary<int, IList<JobHistory>>();

			var jobsInQueue = GetJobsInQueue();

			foreach (var workspaceArtifactId in workspaceArtifactIds)
			{
				var unfinishedJobs = _unfinishedJobService.GetUnfinishedJobs(workspaceArtifactId);
				var jobsWithInvalidStatus = GetJobsWithInvalidStatus(unfinishedJobs, jobsInQueue, workspaceArtifactId);
				if (jobsWithInvalidStatus.Any())
				{
					invalidJobs.Add(workspaceArtifactId, jobsWithInvalidStatus);
				}
			}

			return invalidJobs;
		}

		private Dictionary<Job, TaskParameters> GetJobsInQueue()
		{
			var allScheduledJobs = _jobService.GetAllScheduledJobs();
			return allScheduledJobs.ToDictionary(x => x, y => _serializer.Deserialize<TaskParameters>(y.JobDetails));
		}

		private static List<JobHistory> GetJobsWithInvalidStatus(IList<JobHistory> unfinishedJobs, Dictionary<Job, TaskParameters> jobsInQueue, int workspaceArtifactId)
		{
			var jobsWithInvalidStatus = new List<JobHistory>();
			foreach (var unfinishedJob in unfinishedJobs)
			{
				if (jobsInQueue.Where(x => x.Key.WorkspaceID == workspaceArtifactId)
					.All(x => !string.Equals(x.Value.BatchInstance.ToString(), unfinishedJob.BatchInstance, StringComparison.InvariantCultureIgnoreCase)))
				{
					jobsWithInvalidStatus.Add(unfinishedJob);
				}
			}
			return jobsWithInvalidStatus;
		}
	}
}