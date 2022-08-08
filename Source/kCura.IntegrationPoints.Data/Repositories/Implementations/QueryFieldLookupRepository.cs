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
        protected internal readonly Dictionary<int, ViewFieldInfo> ViewFieldsInfoCache;

        public QueryFieldLookupRepository(IQueryFieldLookup queryFieldLookup, IExternalServiceInstrumentationProvider instrumentationProvider)
        {
            _queryFieldLookup = queryFieldLookup;
            _instrumentationProvider = instrumentationProvider;
            ViewFieldsInfoCache = new Dictionary<int, ViewFieldInfo>();
        }

        /// <inheritdoc />
        public ViewFieldInfo GetFieldByArtifactID(int fieldArtifactID)
        {
            return GetCachedResult(fieldArtifactID);
        }

        /// <inheritdoc />
        public FieldTypeHelper.FieldType GetFieldTypeByArtifactID(int fieldArtifactID)
        {
            return GetCachedResult(fieldArtifactID).FieldType;
        }
        
        private ViewFieldInfo GetCachedResult(int fieldArtifactID)
        {
            if (ViewFieldsInfoCache.TryGetValue(fieldArtifactID, out var viewFieldInfo))
            {
                return viewFieldInfo;
            }

            viewFieldInfo = RunQueryForViewFieldInfo(fieldArtifactID);
            ViewFieldsInfoCache.Add(fieldArtifactID, viewFieldInfo); 

            return viewFieldInfo;
        }

        /// <inheritdoc />
        public virtual ViewFieldInfo RunQueryForViewFieldInfo(int fieldArtifactID)
        {
            IExternalServiceSimpleInstrumentation instrumentation = _instrumentationProvider.CreateSimple(
                ExternalServiceTypes.RELATIVITY_DATA, 
                nameof(IQueryFieldLookup), 
                nameof(IQueryFieldLookup.GetFieldByArtifactID));
            ViewFieldInfo viewFieldInfo = instrumentation.Execute(() => _queryFieldLookup.GetFieldByArtifactID(fieldArtifactID));
            return viewFieldInfo;
        }
    }
}
