using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
    public class ProductionInformationQueryBuilder : QueryBuilder
    {
        public ProductionInformationQueryBuilder AddProductionSetCondition(int productionSetId)
        {
            string productionSetCondition = $"'ProductionSet' == OBJECT {productionSetId}";
            Conditions.Add(productionSetCondition);

            return this;
        }

        public ProductionInformationQueryBuilder AddHasNativeCondition()
        {
            string condition = $"'{ProductionConsts.WithNativesFieldName}' == true";
            Conditions.Add(condition);

            return this;
        }

        public override QueryRequest Build()
        {
            return new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ProductionConsts.ProductionInformationTypeGuid
                },
                Fields = Fields,
                Condition = BuildCondition()
            };
        }

        public ProductionInformationQueryBuilder NoFields()
        {
            Fields = new List<FieldRef>();
            return this;
        }

        public ProductionInformationQueryBuilder AddField(Guid fieldGuid)
        {
            Fields.Add(new FieldRef { Guid = fieldGuid });

            return this;
        }

        public ProductionInformationQueryBuilder AddFields(List<Guid> fieldGuids)
        {
            Fields.AddRange(fieldGuids.Select(x => new FieldRef { Guid = x }));

            return this;
        }
    }
}
