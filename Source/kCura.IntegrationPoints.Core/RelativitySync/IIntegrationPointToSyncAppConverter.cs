using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.RelativitySync
{
    public interface IIntegrationPointToSyncAppConverter
    {
        Task<int> CreateSyncConfigurationAsync(int workspaceId, IntegrationPointDto integrationPointDto, int jobHistoryId, int userId);
    }
}
