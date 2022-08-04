using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public class FieldsRepository : IFieldsRepository
    {
        private readonly IServicesMgr _servicesMgr;

        public FieldsRepository(IServicesMgr servicesMgr)
        {
            _servicesMgr = servicesMgr;
        }

        public async Task<IEnumerable<FieldInfo>> GetAllFieldsAsync(int workspaceId, int artifactTypeId)
        {
            QueryRequest queryRequest = PrepareFieldsQueryRequest($"'FieldArtifactTypeID' == {artifactTypeId}");
            IEnumerable<RelativityObject> fieldObjects = await GetFieldsByQueryAsync(workspaceId, queryRequest).ConfigureAwait(false);

            return fieldObjects.Select(FieldConvert.ToDocumentFieldInfo);
        }

        public async Task<IEnumerable<FieldInfo>> GetFieldsByArtifactsIdAsync(IEnumerable<string> artifactIds, int workspaceId)
        {
            artifactIds = artifactIds?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if(artifactIds == null || !artifactIds.Any())
            {
                return Enumerable.Empty<FieldInfo>();
            }

            QueryRequest queryRequest = PrepareFieldsQueryRequest($"'ArtifactID' IN [{string.Join(",", artifactIds)}]");
            IEnumerable<RelativityObject> fieldObjects = await GetFieldsByQueryAsync(workspaceId, queryRequest).ConfigureAwait(false);

            return fieldObjects.Select(FieldConvert.ToDocumentFieldInfo);
        }

        private QueryRequest PrepareFieldsQueryRequest(string query)
        {
            int fieldArtifactTypeID = (int)ArtifactType.Field;
            QueryRequest queryRequest = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = fieldArtifactTypeID
                },
                Condition = query,
                Fields = new[]
                {
                        new FieldRef()
                        {
                            Name = "*"
                        }
                    },
                IncludeNameInQueryResult = true
            };

            return queryRequest;
        }

        private async Task<IEnumerable<RelativityObject>> GetFieldsByQueryAsync(int workspaceID, QueryRequest queryRequest)
        {
            using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                const int queryBatchSize = 50;
                int resultCount = 0;
                List<RelativityObject> retrievedObjects = new List<RelativityObject>();

                do
                {
                    QueryResult queryResult = await objectManager.QueryAsync(workspaceID, queryRequest,
                        start: retrievedObjects.Count + 1, length: queryBatchSize).ConfigureAwait(false);
                    retrievedObjects.AddRange(queryResult.Objects);
                    resultCount = queryResult.ResultCount;
                } while (resultCount > 0);

                return retrievedObjects;
            }
        }
    }
}
