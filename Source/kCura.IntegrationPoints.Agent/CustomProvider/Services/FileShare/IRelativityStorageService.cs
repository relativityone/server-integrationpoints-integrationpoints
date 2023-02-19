using System.IO;
using System.Threading.Tasks;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare
{
    public interface IRelativityStorageService
    {
        Task<IStorageAccess<string>> GetStorageAccessAsync();

        Task<string> GetWorkspaceDirectoryPathAsync(int workspaceId);
    }
}
