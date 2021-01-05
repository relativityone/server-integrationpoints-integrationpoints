using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobRepository : IJobRepository
	{
		private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

		public JobRepository(IRelativityObjectManagerFactory relativityObjectManagerFactory)
		{
			_relativityObjectManagerFactory = relativityObjectManagerFactory;
		}

		public IList<RDO> GetRunningJobs(int workspaceArtifactId)
		{
			IRelativityObjectManager objectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId);

			var unfinishedJobsStatusesGuids = new List<Guid>
			{
				JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault(),
				JobStatusChoices.JobHistoryValidating.Guids.FirstOrDefault()
			};

			var request = new QueryRequest
			{
				Condition = $"'{JobHistoryFields.JobStatus}' IN CHOICE [{string.Join(",", unfinishedJobsStatusesGuids)}]"
			};

			List<JobHistory> results = objectManager.Query<JobHistory>(request, ExecutionIdentity.System);

			return results.Select(x => x.Rdo).ToList();
		}

		public IList<JobHistory> GetStuckJobs(IList<int> stuckJobsIds, int workspaceId)
		{
			IRelativityObjectManager rsapiService = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId);
			QueryRequest request = new QueryRequest
			{
				Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' in [{string.Join(",", stuckJobsIds)}]",
				Fields = new JobHistory().ToFieldList()
			};
			return rsapiService.Query<JobHistory>(request);
		}
	}
}