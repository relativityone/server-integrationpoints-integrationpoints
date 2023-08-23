using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.AdlsHelpers
{
    public interface IAdlsHelper
    {
        Task<bool> IsWorkspaceMigratedToAdlsAsync(int workspaceId);

        void AddToFileShareStatistics(string fileLocation);

        Task LogFileSharesSummaryAsync();
    }
}