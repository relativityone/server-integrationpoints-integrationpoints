using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Management.Tasks.Helpers
{
	public class StuckJobs : IStuckJobs
	{
		private const int _STUCK_TIME_IN_HOURS = 1;

		private readonly IJobService _jobService;
		private readonly IJobRepository _jobRepository;

		public StuckJobs(IJobService jobService, IJobRepository jobRepository)
		{
			_jobService = jobService;
			_jobRepository = jobRepository;
		}

		public IDictionary<int, IList<JobHistory>> FindStuckJobs(IList<int> workspaceArtifactIds)
		{
			List<Job> pickedUpJobs = GetPickedUpJobs();

			var result = new Dictionary<int, IList<JobHistory>>();

			foreach (var workspaceId in pickedUpJobs.Select(x => x.WorkspaceID).Distinct())
			{
				IList<RDO> jobsInProgress = _jobRepository.GetRunningJobs(workspaceId);

				List<string> pickedUpJobsBatchInstances = pickedUpJobs.Where(x => x.WorkspaceID == workspaceId)
					.Select(x => JsonConvert.DeserializeObject<TaskParameters>(x.JobDetails).BatchInstance.ToString()).ToList();

				IList<int> stuckJobsIds = GetStuckJobsIds(jobsInProgress, pickedUpJobsBatchInstances);
				if (stuckJobsIds.Count > 0)
				{
					IList<JobHistory> stuckJobs = _jobRepository.GetStuckJobs(stuckJobsIds, workspaceId);
					result.Add(workspaceId, stuckJobs);
				}
			}

			return result;
		}

		private List<Job> GetPickedUpJobs()
		{
			IEnumerable<Job> scheduledJobs = _jobService.GetAllScheduledJobs();
			return scheduledJobs.Where(x => x.LockedByAgentID.HasValue).ToList();
		}

		private IList<int> GetStuckJobsIds(IList<RDO> jobsInProgress, List<string> jobs)
		{
			return jobsInProgress
				.Where(x => jobs.Any(y => string.Equals(y, x[new Guid(JobHistoryFieldGuids.BatchInstance)].ValueAsFixedLengthText, StringComparison.InvariantCultureIgnoreCase)))
				.Where(x => x.SystemLastModifiedOn < GetStuckTime())
				.Select(x => x.ArtifactID).ToList();
		}

		private static DateTime GetStuckTime()
		{
			return DateTime.UtcNow.AddHours(-_STUCK_TIME_IN_HOURS);
		}
	}
}