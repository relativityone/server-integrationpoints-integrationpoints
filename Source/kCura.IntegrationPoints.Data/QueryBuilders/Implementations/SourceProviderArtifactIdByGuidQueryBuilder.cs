using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
    public class SourceProviderArtifactIdByGuidQueryBuilder : ISourceProviderArtifactIdByGuidQueryBuilder
    {
        public QueryRequest Create(string guid)
        {
            return new QueryRequest
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{guid}'",
                Fields = new List<FieldRef>() { new FieldRef { Name = Domain.Constants.SOURCEPROVIDER_ARTIFACTID_FIELD_NAME } },
                ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.SourceProvider) }
            };
        }
    }
}