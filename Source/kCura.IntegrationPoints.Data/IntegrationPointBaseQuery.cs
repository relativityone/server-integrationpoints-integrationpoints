using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public class IntegrationPointBaseQuery<T> where T : BaseRdo, new()
	{
		private readonly IRSAPIService _context;

		public IntegrationPointBaseQuery(IRSAPIService context)
		{
			_context = context;
		}

		public IList<T> GetIntegrationPoints(List<int> sourceProviderIds)
		{
			var query = new Query<RDO>
			{
				Fields = new List<FieldValue>
				{
					new FieldValue(IntegrationPointFields.Name)
				},
				Condition = new WholeNumberCondition(
					IntegrationPointFields.SourceProvider, NumericConditionEnum.In, sourceProviderIds)
			};

			IList<T> result = _context.GetGenericLibrary<T>().Query(query);
			return result;
		}

		public IList<T> GetAllIntegrationPoints()
		{
			var query = new Query<RDO>
			{
				Fields = GetFields().ToList()
			};

			IList<T> result = _context.GetGenericLibrary<T>().Query(query);

			return result;
		}

		public IList<T> GetAllIntegrationPointsProfileWithBasicColumns()
		{
			var query = new Query<RDO>
			{
				Fields = GetBasicProfileFields().ToList()
			};

			IList<T> result = _context.GetGenericLibrary<T>().Query(query);

			return result;
		}

		private IEnumerable<FieldValue> GetFields()
		{
			return BaseRdo.GetFieldMetadata(typeof(T)).Values.ToList().Select(field => new FieldValue(field.FieldGuid));
		}

		private IEnumerable<FieldValue> GetBasicProfileFields()
		{
			return BaseRdo.GetFieldMetadata(typeof(T)).Values.ToList().Select(field => new FieldValue(field.FieldGuid))
				.Where(field => field.Guids.Contains(new Guid(IntegrationPointProfileFieldGuids.DestinationProvider)) ||
				field.Guids.Contains(new Guid(IntegrationPointProfileFieldGuids.SourceProvider)) ||
				field.Guids.Contains(new Guid(IntegrationPointProfileFieldGuids.Name)) ||
				field.Guids.Contains(new Guid(IntegrationPointProfileFieldGuids.Type)));
		}
	}
}