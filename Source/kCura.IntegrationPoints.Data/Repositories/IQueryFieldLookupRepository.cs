

using Relativity;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IQueryFieldLookupRepository
    {
        ViewFieldInfo GetFieldByArtifactId(int fieldArtifactId);
    }
}
