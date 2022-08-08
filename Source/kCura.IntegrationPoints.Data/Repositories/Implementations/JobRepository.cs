using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.Services;
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

        public IList<RelativityObject> GetRunningJobs(int workspaceArtifactId)
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

            List<RelativityObject> results = objectManager.Query(request, ExecutionIdentity.System);
            return results;
        }

        public IList<JobHistory> GetStuckJobs(IList<int> stuckJobsIds, int workspaceId)
        {
            IRelativityObjectManager relativityObjectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId);
            QueryRequest request = new QueryRequest
            {
                Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' in [{string.Join(",", stuckJobsIds)}]",
                Fields = new JobHistory().ToFieldList()
            };
            return relativityObjectManager.Query<JobHistory>(request);
        }
    }
}