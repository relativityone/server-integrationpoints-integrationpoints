using System;
using Relativity;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class QueryFieldLookupRepository : IQueryFieldLookupRepository
	{
		private readonly IQueryFieldLookup _queryFieldLookup;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		protected internal readonly Dictionary<int, ViewFieldInfoFieldTypeExtender> ViewFieldsInfoCache;

		public QueryFieldLookupRepository(IQueryFieldLookup queryFieldLookup, IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_queryFieldLookup = queryFieldLookup;
			_instrumentationProvider = instrumentationProvider;
			ViewFieldsInfoCache = new Dictionary<int, ViewFieldInfoFieldTypeExtender>();
		}

		/// <inheritdoc />
		public ViewFieldInfo GetFieldByArtifactId(int fieldArtifactId)
		{
			return GetCachedResult(fieldArtifactId).Value;
		}

		/// <inheritdoc />
		public string GetFieldTypeByArtifactId(int fieldArtifactId)
		{
			return GetCachedResult(fieldArtifactId).FieldTypeAsString;
		}
		
		private ViewFieldInfoFieldTypeExtender GetCachedResult(int fieldArtifactId)
		{
			ViewFieldInfoFieldTypeExtender viewFieldInfo;
			if (ViewFieldsInfoCache.TryGetValue(fieldArtifactId, out viewFieldInfo))
			{
				return viewFieldInfo;
			}

			viewFieldInfo = RunQueryForViewFieldInfo(fieldArtifactId);
			ViewFieldsInfoCache.Add(fieldArtifactId, viewFieldInfo); 

			return viewFieldInfo;
		}

		/// <inheritdoc />
		public virtual ViewFieldInfoFieldTypeExtender RunQueryForViewFieldInfo(int fieldArtifactId)
		{
			IExternalServiceSimpleInstrumentation instrumentation = _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.RELATIVITY_DATA, 
				nameof(IQueryFieldLookup), 
				nameof(IQueryFieldLookup.GetFieldByArtifactID));
			ViewFieldInfo viewFieldInfo = instrumentation.Execute(() => _queryFieldLookup.GetFieldByArtifactID(fieldArtifactId));
			return new ViewFieldInfoFieldTypeExtender(viewFieldInfo);
		}
	}
}
