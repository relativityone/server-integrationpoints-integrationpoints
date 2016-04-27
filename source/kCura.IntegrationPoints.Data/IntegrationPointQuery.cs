using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Attributes;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Data;

namespace kCura.IntegrationPoints.Data
{
	public class IntegrationPointQuery
	{
		private readonly IRSAPIService _context;

		public IntegrationPointQuery(IRSAPIService context)
		{
			_context = context;
		}

		public IList<IntegrationPoint> GetIntegrationPoints(List<int> sourceProviderIds)
		{
			var query = new Query<RDO>
			{
				Fields = new List<FieldValue>
				{
					new FieldValue(Guid.Parse(IntegrationPointFieldGuids.Name))
				},
				Condition = new WholeNumberCondition(
					IntegrationPointFields.SourceProvider, NumericConditionEnum.In, sourceProviderIds)
			};

			IList<IntegrationPoint> result = _context.IntegrationPointLibrary.Query(query);
			return result;
		}

		public IList<IntegrationPoint> GetAllIntegrationPoints()
		{
			var query = new Query<RDO>
			{
				Fields = GetFields().ToList()
			};

			IList<IntegrationPoint> result = _context.IntegrationPointLibrary.Query(query);
			return result;
		}

		private IEnumerable<FieldValue> GetFields()
		{
			return BaseRdo.GetFieldMetadata(typeof(IntegrationPoint)).Values.ToList().Select(field => new FieldValue(field.FieldGuid));
		}
	}
}
