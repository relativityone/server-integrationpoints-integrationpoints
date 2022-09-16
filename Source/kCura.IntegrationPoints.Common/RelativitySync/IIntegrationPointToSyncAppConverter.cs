using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.RelativitySync
{
    public interface IIntegrationPointToSyncAppConverter
    {
        Task<int> CreateSyncConfigurationAsync(int workspaceId, int integrationPointId, int jobHistoryId, int userId);
    }
}