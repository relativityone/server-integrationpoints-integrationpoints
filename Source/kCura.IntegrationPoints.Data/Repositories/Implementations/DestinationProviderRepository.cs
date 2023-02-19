using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class DestinationProviderRepository : Repository<DestinationProvider>, IDestinationProviderRepository
    {
        private readonly IAPILog _logger;
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IDestinationProviderArtifactIdByGuidQueryBuilder _artifactIdByGuid = new DestinationProviderArtifactIdByGuidQueryBuilder();

        public DestinationProviderRepository(IAPILog logger, IRelativityObjectManager relativityObjectManager)
        : base(relativityObjectManager)
        {
            _logger = logger.ForContext<DestinationProviderRepository>();
            _relativityObjectManager = relativityObjectManager;
        }

        public int GetArtifactIdFromDestinationProviderTypeGuidIdentifier(string destinationProviderGuidIdentifier)
        {
            QueryRequest query = _artifactIdByGuid.Create(destinationProviderGuidIdentifier);

            List<RelativityObject> queryResults;
            try
            {
                queryResults = _relativityObjectManager.Query(query);
                return queryResults.Single().ArtifactID;
            }
            catch (Exception e)
            {
                throw new IntegrationPointsException($"Failed to retrieve Destination Provider Artifact Id for guid: {destinationProviderGuidIdentifier}.", e);
            }
        }

        public DestinationProvider ReadByProviderGuid(string providerGuid)
        {
            var queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = Guid.Parse(ObjectTypeGuids.DestinationProvider)
                },
                Fields = RDOConverter.GetFieldList<DestinationProvider>(),
                Condition = $"'{DestinationProviderFields.Identifier}' == '{providerGuid}'"
            };
            IList<DestinationProvider> destinationProviders = _relativityObjectManager.Query<DestinationProvider>(queryRequest);

            if (destinationProviders.Count > 1)
            {
                LogMoreThanOneProviderFoundWarning(providerGuid);
            }
            return destinationProviders.SingleOrDefault();
        }

        private void LogMoreThanOneProviderFoundWarning(string providerGuid)
        {
            _logger.LogWarning("More than one Destination Provider with {GUID} found.", providerGuid);
        }
    }
}
