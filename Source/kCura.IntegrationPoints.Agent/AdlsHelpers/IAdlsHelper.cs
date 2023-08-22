using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.AdlsHelpers
{
    public interface IAdlsHelper
    {
        Task<bool> IsWorkspaceMigratedToAdlsAsync(int workspaceId);
    }
}