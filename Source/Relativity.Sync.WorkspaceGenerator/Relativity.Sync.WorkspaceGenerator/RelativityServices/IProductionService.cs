using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
    public interface IProductionService
    {
        Task<int?> GetProductionIdAsync(int workspaceId, string productionName);
        Task<int> CreateProductionAsync(int workspaceId, string productionName);
    }
}
