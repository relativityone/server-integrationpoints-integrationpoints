using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IIntegrationPointRepository
    {
        Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID);
        Task<IntegrationPoint> ReadWithFieldMappingAsync(int integrationPointArtifactID);
        Task<IntegrationPoint> ReadEncryptedAsync(int integrationPointArtifactID);
        Task<List<FieldMap>> GetFieldMappingAsync(int integrationPointArtifactID);
        string GetSecuredConfiguration(int integrationPointArtifactID);
        string GetName(int integrationPointArtifactID);
        int CreateOrUpdate(IntegrationPoint integrationPoint);
        void Update(IntegrationPoint integrationPoint);
        void UpdateHasErrors(int integrationPointArtifactId, bool hasErrors);
        void Delete(int integrationPointID);

        List<IntegrationPoint> GetAll(List<int> integrationPointIDs);
        Task<List<IntegrationPoint>> GetBySourceAndDestinationProviderAsync(
            int sourceProviderArtifactID,
            int destinationProviderArtifactID);
        List<IntegrationPoint> GetIntegrationPoints(List<int> sourceProviderIds);
        List<IntegrationPoint> GetIntegrationPointsWithAllFields();
    }
}
