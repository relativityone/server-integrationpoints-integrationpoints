using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Management.Tasks.Helpers
{
	public class StuckJobs : IStuckJobs
	{
		private const int _STUCK_TIME_IN_HOURS = 1;

		private readonly IJobService _jobService;
		private readonly IRunningJobService _runningJobService;
		private readonly IRSAPIServiceFactory _rsapiServiceFactory;

		public StuckJobs(IJobService jobService, IRunningJobService runningJobService, IRSAPIServiceFactory rsapiServiceFactory)
		{
			_jobService = jobService;
			_runningJobService = runningJobService;
			_rsapiServiceFactory = rsapiServiceFactory;
		}

		public IDictionary<int, IList<JobHistory>> FindStuckJobs(IList<int> workspaceArtifactIds)
		{
			var pickedUpJobs = GetPickedUpJobs();

			var result = new Dictionary<int, IList<JobHistory>>();

			foreach (var workspaceId in pickedUpJobs.Select(x => x.WorkspaceID).Distinct())
			{
				var jobsInProgress = _runningJobService.GetRunningJobs(workspaceId);

				List<string> pickedUpJobsBatchInstances = pickedUpJobs.Where(x => x.WorkspaceID == workspaceId)
					.Select(x => JsonConvert.DeserializeObject<TaskParameters>(x.JobDetails).BatchInstance.ToString()).ToList();

				var stuckJobsIds = GetStuckJobsIds(jobsInProgress, pickedUpJobsBatchInstances);
				if (stuckJobsIds.Count > 0)
				{
					var stuckJobs = GetStuckJobs(stuckJobsIds, workspaceId);
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

		private IList<int> GetStuckJobsIds(List<RDO> jobsInProgress, List<string> jobs)
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

		private IList<JobHistory> GetStuckJobs(IList<int> stuckJobsIds, int workspaceId)
		{
			var rsapiService = _rsapiServiceFactory.Create(workspaceId);
			return rsapiService.JobHistoryLibrary.Read(stuckJobsIds);
		}
	}
}