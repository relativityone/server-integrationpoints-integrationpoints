using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare
{
    public interface IFileShareService
    {
        Task<string> GetWorkspaceFileShareLocationAsync(int workspaceId);
    }
}