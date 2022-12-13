using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IIntegrationPointRepository
    {
        Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID);

        Task<string> GetFieldMappingAsync(int integrationPointArtifactID);

        Task<string> GetSourceConfigurationAsync(int integrationPointArtifactID);

        Task<string> GetDestinationConfigurationAsync(int integrationPointArtifactID);

        string GetEncryptedSecuredConfiguration(int integrationPointArtifactID);

        string GetName(int integrationPointArtifactID);

        int CreateOrUpdate(IntegrationPoint integrationPoint);

        void UpdateType(int artifactId, int? type);

        void UpdateHasErrors(int integrationPointArtifactId, bool hasErrors);

        void UpdateLastAndNextRunTime(int artifactId, DateTime? lastRuntime, DateTime? nextRuntime);

        void DisableScheduler(int artifactId);

        void UpdateJobHistory(int artifactId, List<int> jobHistory);

        void UpdateSourceConfiguration(int artifactId, string sourceConfiguration);

        void UpdateDestinationConfiguration(int artifactId, string destinationConfiguration);

        void Delete(int integrationPointID);

        Task<List<IntegrationPoint>> ReadBySourceAndDestinationProviderAsync(
            int sourceProviderArtifactID,
            int destinationProviderArtifactID);

        List<IntegrationPoint> ReadBySourceProviders(List<int> sourceProviderIds);

        List<IntegrationPoint> ReadAll();
    }
}
