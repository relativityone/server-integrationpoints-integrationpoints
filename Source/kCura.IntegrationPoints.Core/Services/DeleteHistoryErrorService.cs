using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services
{
    public class DeleteHistoryErrorService : IDeleteHistoryErrorService
    {
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

        public DeleteHistoryErrorService(IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
        }

        public void DeleteErrorAssociatedWithHistories(List<int> historiesId, int workspaceArtifactId)
        {
            IRelativityObjectManager objectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId);

            var query = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = Guid.Parse(ObjectTypeGuids.JobHistoryError)
                },
                Fields = Enumerable.Empty<FieldRef>(),
                Condition = CreateCondition(historiesId)
            };

            List<JobHistoryError> result = objectManager.Query<JobHistoryError>(query);
            List<int> allJobHistoryError = result.Select(x => x.ArtifactId).ToList();
            objectManager.MassDeleteAsync(allJobHistoryError).GetAwaiter().GetResult();
        }

        private string CreateCondition(List<int> historiesId)
        {
            string historiesIdList = string.Join(",", historiesId.Select(x => x.ToString()));
            return $"'{JobHistoryErrorFields.JobHistory}' IN OBJECT [{historiesIdList}]";
        }
    }
}
