using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
    public class DestinationProviderArtifactIdByGuidQueryBuilder : IDestinationProviderArtifactIdByGuidQueryBuilder
    {
        public QueryRequest Create(string guid)
        {
            return new QueryRequest
            {
                Condition = $"'{DestinationProviderFields.Identifier}' == '{guid}'",
                Fields = new List<FieldRef>() { new FieldRef { Name = Domain.Constants.DESTINATION_PROVIDER_ARTIFACTID_FIELD_NAME } },
                ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.DestinationProvider) }
            };
        }
    }
}
