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
        Task<IEnumerable<FieldMap>> GetFieldMappingAsync(int integrationPointArtifactID);
        string GetSecuredConfiguration(int integrationPointArtifactID);
        string GetName(int integrationPointArtifactID);
        int CreateOrUpdate(IntegrationPoint integrationPoint);
        void Update(IntegrationPoint integrationPoint);
        void Delete(int integrationPointID);

        IList<IntegrationPoint> GetAll(List<int> integrationPointIDs);
        Task<IList<IntegrationPoint>> GetAllBySourceAndDestinationProviderIDsAsync(
            int sourceProviderArtifactID,
            int destinationProviderArtifactID);
        IList<IntegrationPoint> GetIntegrationPoints(List<int> sourceProviderIds);
        IList<IntegrationPoint> GetAllIntegrationPoints();
        IList<IntegrationPoint> GetIntegrationPointsWithAllFields();
    }
}