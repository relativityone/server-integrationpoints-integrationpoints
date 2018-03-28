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
		private readonly IRSAPIServiceFactory _rsapiServiceFactory;

		public JobRepository(IRSAPIServiceFactory rsapiServiceFactory)
		{
			_rsapiServiceFactory = rsapiServiceFactory;
		}

		public IList<RDO> GetRunningJobs(int workspaceArtifactId)
		{
			IRSAPIService rsapiService = _rsapiServiceFactory.Create(workspaceArtifactId);

			var unfinishedJobsStatusesGuids = new List<Guid>
			{
				JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault()
			};

			var request = new QueryRequest
			{
				Condition = $"'{JobHistoryFields.JobStatus}' IN CHOICE [{string.Join(",", unfinishedJobsStatusesGuids)}]"
			};

			List<JobHistory> results = rsapiService.RelativityObjectManager.Query<JobHistory>(request, ExecutionIdentity.System);

			return results.Select(x => x.Rdo).ToList();
		}

		public IList<JobHistory> GetStuckJobs(IList<int> stuckJobsIds, int workspaceId)
		{
			IRSAPIService rsapiService = _rsapiServiceFactory.Create(workspaceId);
			QueryRequest request = new QueryRequest
			{
				Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' in [{string.Join(",", stuckJobsIds)}]",
				Fields = new JobHistory().ToFieldList()
			};
			return rsapiService.RelativityObjectManager.Query<JobHistory>(request);
		}
	}
}