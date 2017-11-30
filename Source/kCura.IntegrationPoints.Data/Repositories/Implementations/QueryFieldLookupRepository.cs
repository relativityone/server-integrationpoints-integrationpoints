using Relativity;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class QueryFieldLookupRepository : IQueryFieldLookupRepository
    {
        private readonly BaseServiceContext _context;

        public QueryFieldLookupRepository(BaseServiceContext context)
        {
            _context = context;
        }

        public ViewFieldInfo GetFieldByArtifactId(int fieldArtifactId)
        {
            IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(_context, (int)ArtifactType.Document);
            return fieldLookupHelper.GetFieldByArtifactID(fieldArtifactId);
        }
    }
}
