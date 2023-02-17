using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class UnlinkedJobHistoryService : IUnlinkedJobHistoryService
    {
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

        public UnlinkedJobHistoryService(IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
        }

        public List<int> FindUnlinkedJobHistories(int workspaceArtifactId)
        {
            IRelativityObjectManager objectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId);

            var request = new QueryRequest
            {
                Condition = $"NOT '{JobHistoryFields.IntegrationPoint}' ISSET"
            };
            return objectManager.Query<Data.JobHistory>(request).Select(x => x.ArtifactId).ToList();
        }
    }
}
