using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class SourceProviderRepository : Repository<SourceProvider>, ISourceProviderRepository
    {
        private readonly ISourceProviderArtifactIdByGuidQueryBuilder _artifactIdByGuid = new SourceProviderArtifactIdByGuidQueryBuilder();
        private readonly IRelativityObjectManager _relativityObjectManager;

        public SourceProviderRepository(IRelativityObjectManager relativityObjectManager)
        : base(relativityObjectManager)
        {
            _relativityObjectManager = relativityObjectManager;
        }

        public int GetArtifactIdFromSourceProviderTypeGuidIdentifier(
            string sourceProviderGuidIdentifier,
            ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
        {
            QueryRequest query = _artifactIdByGuid.Create(sourceProviderGuidIdentifier);

            try
            {
                List<RelativityObject> queryResults = _relativityObjectManager.Query(query, executionIdentity);
                return queryResults.Single().ArtifactID;
            }
            catch (Exception e)
            {
                throw new IntegrationPointsException($"Failed to retrieve Source Provider Artifact Id for guid: {sourceProviderGuidIdentifier}", e);
            }
        }

        public Task<List<SourceProvider>> GetSourceProviderRdoByApplicationIdentifierAsync(
            Guid appGuid,
            ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
        {
            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = Guid.Parse(ObjectTypeGuids.SourceProvider)
                },
                Fields = RDOConverter.ConvertPropertiesToFields<SourceProvider>(),
                Condition = $"'{SourceProviderFields.ApplicationIdentifier}' == '{appGuid}'"
            };

            return _relativityObjectManager.QueryAsync<SourceProvider>(request, executionIdentity: executionIdentity);
        }
    }
}