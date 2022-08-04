using Relativity.Sync;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync
{
    public interface IIntegrationPointToSyncConverter
    {
        Task<int> CreateSyncConfigurationAsync(IExtendedJob job);
    }
}
