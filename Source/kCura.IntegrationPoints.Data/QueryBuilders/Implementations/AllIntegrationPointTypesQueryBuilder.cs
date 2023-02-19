using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
    public class AllIntegrationPointTypesQueryBuilder
    {
        public QueryRequest Create()
        {
            return new QueryRequest
            {
                ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.IntegrationPointType) },
                Fields = new List<FieldRef>
                {
                    new FieldRef {Guid = new Guid(IntegrationPointTypeFieldGuids.Name) },
                    new FieldRef { Guid = new Guid(IntegrationPointTypeFieldGuids.Identifier)}
                }
            };
        }
    }
}
