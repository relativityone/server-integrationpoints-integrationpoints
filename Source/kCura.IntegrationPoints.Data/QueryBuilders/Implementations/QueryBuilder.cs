using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public abstract class QueryBuilder
	{
		protected IList<Condition> Conditions { get; }
		protected List<FieldValue> Fields { get; set; }

		protected QueryBuilder()
		{
			Conditions = new List<Condition>();
			Fields = new List<FieldValue>();
		}

		public abstract Query<RDO> Build();

		protected Condition BuildCondition()
		{
			if (Conditions.Count == 0)
			{
				return null;
			}
			var currentCondition = Conditions.First();
			for (int i = 1; i < Conditions.Count; i++)
			{
				currentCondition = new CompositeCondition(currentCondition, CompositeConditionEnum.And, Conditions[i]);
			}
			return currentCondition;
		}
	}
}