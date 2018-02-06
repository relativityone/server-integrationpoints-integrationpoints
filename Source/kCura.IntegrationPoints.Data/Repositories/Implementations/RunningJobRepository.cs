using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RunningJobRepository : IRunningJobRepository
	{
		private IRelativityObjectManager _objectManager;

		public RunningJobRepository(IRelativityObjectManager objectManager)
		{
			_objectManager = objectManager;
		}

		public List<RDO> GetRunningJobs(int workspaceArtifactId)
		{
			var unfinishedJobsStatusesGuids = new List<Guid>
			{
				JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault()
			};
			var results = _objectManager.Query<JobHistory>(new QueryRequest
			{
				Condition = $"'{JobHistoryFields.JobStatus}' IN CHOICE [{string.Join(",", unfinishedJobsStatusesGuids)}]"
			}, ExecutionIdentity.System);
			return results.Select(x => x.Rdo).ToList();
		}
	}
}