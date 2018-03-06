using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;

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
			var sourceProviderQuery = GetBasicSourceProviderQuery(sourceProviderIds);

			sourceProviderQuery.Fields = new List<FieldRef>
			{
				new FieldRef {Name = IntegrationPointFields.Name}
			};

			IList<T> result = _context.RelativityObjectManager.Query<T>(sourceProviderQuery);
			return result;
		}

		public IList<T> GetIntegrationPointsWithAllFields(List<int> sourceProviderIds)
		{
			var sourceProviderQuery = GetBasicSourceProviderQuery(sourceProviderIds);

			IList<T> result = _context.RelativityObjectManager.Query<T>(sourceProviderQuery);
			return result;
		}

		public IList<T> GetAllIntegrationPoints()
		{
			var query = new QueryRequest()
			{
				Fields = GetFields()
			};

			IList<T> result = _context.RelativityObjectManager.Query<T>(query);

			return result;
		}

		public IList<T> GetAllIntegrationPointsProfileWithBasicColumns()
		{
			var query = new QueryRequest()
			{
				Fields = GetBasicProfileFields()
			};

			IList<T> result = _context.RelativityObjectManager.Query<T>(query);

			return result;
		}

		private IEnumerable<FieldRef> GetFields()
		{
			return BaseRdo.GetFieldMetadata(typeof(T)).Values.ToList().Select(field => new FieldRef { Guid = field.FieldGuid });
		}

		private IEnumerable<FieldRef> GetBasicProfileFields()
		{
			return BaseRdo.GetFieldMetadata(typeof(T)).Values.ToList()
				.Select(field => new FieldValue(field.FieldGuid))
				.Where(field => field.Guids.Contains(new Guid(IntegrationPointProfileFieldGuids.DestinationProvider)) ||
								field.Guids.Contains(new Guid(IntegrationPointProfileFieldGuids.SourceProvider)) ||
								field.Guids.Contains(new Guid(IntegrationPointProfileFieldGuids.Name)) ||
								field.Guids.Contains(new Guid(IntegrationPointProfileFieldGuids.Type)))
					.Select(field => new FieldRef { Guid = field.Guids.First() });
		}

		private QueryRequest GetBasicSourceProviderQuery(List<int> sourceProviderIds)
		{
			return new QueryRequest
			{
				Condition = $"'{IntegrationPointFields.SourceProvider}' in [{string.Join(",", sourceProviderIds)}]"
			};
		}
	}
}