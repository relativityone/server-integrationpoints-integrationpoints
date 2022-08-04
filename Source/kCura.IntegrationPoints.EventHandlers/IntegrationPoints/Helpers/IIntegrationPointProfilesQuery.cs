using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
    internal interface IIntegrationPointProfilesQuery
    {
        Task<IEnumerable<IntegrationPointProfile>> GetAllProfilesAsync(int workspaceID);
        Task<IEnumerable<int>> CheckIfProfilesExistAsync(int workspaceID, IEnumerable<int> artifactIds);
        IEnumerable<IntegrationPointProfile> GetProfilesToUpdate(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID);
        IEnumerable<IntegrationPointProfile> GetProfilesToDelete(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID);
        Task<int> GetSyncDestinationProviderArtifactIDAsync(int workspaceID);
        Task<int> GetSyncSourceProviderArtifactIDAsync(int workspaceID);
        Task<int> GetIntegrationPointExportTypeArtifactIDAsync(int workspaceID);
    }
}