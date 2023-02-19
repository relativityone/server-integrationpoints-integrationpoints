using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
    public interface ISourceProviderArtifactIdByGuidQueryBuilder
    {
        QueryRequest Create(string guid);
    }
}
