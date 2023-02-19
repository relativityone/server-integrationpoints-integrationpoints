using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class RdoStatistics : IRdoStatistics
    {
        private readonly IRelativityObjectManager _relativityObjectManager;

        public RdoStatistics(IRelativityObjectManager relativityObjectManager)
        {
            _relativityObjectManager = relativityObjectManager;
        }

        public int ForView(int artifactTypeId, int viewId)
        {
            var queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    ArtifactTypeID = artifactTypeId
                },
                Condition = $"'ArtifactId' IN VIEW {viewId}"
            };

            return _relativityObjectManager.QueryTotalCount(queryRequest);
        }
    }
}
