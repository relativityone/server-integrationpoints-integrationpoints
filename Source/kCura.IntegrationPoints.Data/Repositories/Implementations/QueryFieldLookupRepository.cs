using Relativity;
using System.Collections.Generic;
using kCura.Data.RowDataGateway;
using Relativity.Data;
using ArtifactType = Relativity.ArtifactType;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class QueryFieldLookupRepository : IQueryFieldLookupRepository
	{
		private readonly BaseContext _workspaceDbContext;
		private readonly int _caseUserArtifactId;
		protected internal readonly Dictionary<int, ViewFieldInfoFieldTypeExtender> ViewFieldsInfoCache;

		public QueryFieldLookupRepository(BaseContext workspaceDbContext, int caseUserArtifactId)
		{
			_workspaceDbContext = workspaceDbContext;
			_caseUserArtifactId = caseUserArtifactId;
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
			IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(_workspaceDbContext, _caseUserArtifactId, (int) ArtifactType.Document);
			return new ViewFieldInfoFieldTypeExtender(fieldLookupHelper.GetFieldByArtifactID(fieldArtifactId));
		}
	}
}
