using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
    public interface IDestinationProviderArtifactIdByGuidQueryBuilder
    {
        QueryRequest Create(string guid);
    }
}
