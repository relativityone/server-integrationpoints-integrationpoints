using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class ProductionInformationQueryBuilder : QueryBuilder
	{
		public ProductionInformationQueryBuilder AddProductionSetCondition(int productionSetId)
		{
			var productionSetCondition = new ObjectCondition("ProductionSet", ObjectConditionEnum.EqualTo, productionSetId);
			Conditions.Add(productionSetCondition);

			return this;
		}

		public ProductionInformationQueryBuilder AddHasNativeCondition()
		{
			var withNativesCondition = new BooleanCondition(ProductionConsts.WithNativesFieldGuid, BooleanConditionEnum.EqualTo, true);
			Conditions.Add(withNativesCondition);

			return this;
		}

		public ProductionInformationQueryBuilder AllFields()
		{
			Fields = FieldValue.AllFields;

			return this;
		}

		public ProductionInformationQueryBuilder NoFields()
		{
			Fields = FieldValue.NoFields;

			return this;
		}

		public ProductionInformationQueryBuilder AddField(Guid fieldGuid)
		{
			Fields.Add(new FieldValue(fieldGuid));

			return this;
		}

		public ProductionInformationQueryBuilder AddFields(List<Guid> fieldGuids)
		{
			Fields.AddRange(fieldGuids.Select(x => new FieldValue(x)));

			return this;
		}

		public override Query<RDO> Build()
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = ProductionConsts.ProductionInformationTypeGuid,
				Fields = Fields,
				Condition = BuildCondition()
			};
		}
	}
}