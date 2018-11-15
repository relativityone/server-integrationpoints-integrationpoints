using Relativity;
using Relativity.Core;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class QueryFieldLookupRepository : IQueryFieldLookupRepository
	{
		private readonly ICoreContext _context;
		protected internal readonly Dictionary<int, ViewFieldInfoFieldTypeExtender> ViewFieldsInfoCache;

		public QueryFieldLookupRepository(ICoreContext context)
		{
			_context = context;
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
			IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(_context, (int)ArtifactType.Document);
			return new ViewFieldInfoFieldTypeExtender(fieldLookupHelper.GetFieldByArtifactID(fieldArtifactId));
		}

	}
}
