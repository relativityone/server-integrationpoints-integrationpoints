using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IIntegrationPointRepository
    {
        Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID);
        Task<IntegrationPoint> ReadEncryptedAsync(int integrationPointArtifactID);
        Task<string> GetFieldMappingAsync(int integrationPointArtifactID);
        string GetEncryptedSecuredConfiguration(int integrationPointArtifactID);
        string GetName(int integrationPointArtifactID);
        int CreateOrUpdate(IntegrationPoint integrationPoint);
        void Update(IntegrationPoint integrationPoint);
        void UpdateHasErrors(int integrationPointArtifactId, bool hasErrors);
        void Delete(int integrationPointID);

        List<IntegrationPoint> ReadAllByIds(List<int> integrationPointIDs);
        Task<List<IntegrationPoint>> ReadBySourceAndDestinationProviderAsync(
            int sourceProviderArtifactID,
            int destinationProviderArtifactID);
        List<IntegrationPoint> ReadBySourceProviders(List<int> sourceProviderIds);
        List<IntegrationPoint> ReadAll();
    }
}
