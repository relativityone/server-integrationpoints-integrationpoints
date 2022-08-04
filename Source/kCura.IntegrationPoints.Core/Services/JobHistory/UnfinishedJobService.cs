using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class UnfinishedJobService : IUnfinishedJobService
    {
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

        public UnfinishedJobService(IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
        }

        public IList<Data.JobHistory> GetUnfinishedJobs(int workspaceArtifactId)
        {
            Guid[] unfinishedChoicesNames = {
                JobStatusChoices.JobHistoryPending.Guids.FirstOrDefault(),
                JobStatusChoices.JobHistoryValidating.Guids.FirstOrDefault(),
                JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault(),
                JobStatusChoices.JobHistoryStopping.Guids.FirstOrDefault()
            };

            var request = new QueryRequest
            {
                Fields = new Data.JobHistory().ToFieldList(),
                Condition = $"'{JobHistoryFields.JobStatus}' IN CHOICE [{string.Join(",", unfinishedChoicesNames)}]"
            };

            return _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId).Query<Data.JobHistory>(request);
        }
    }
}