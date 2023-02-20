using System.IO;
using System.Threading.Tasks;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare
{
    public interface IRelativityStorageService
    {
        Task<IStorageAccess<string>> GetStorageAccessAsync();

        Task<StorageStream> CreateFileOrTruncateExistingAsync(string path);

        Task<string> GetWorkspaceDirectoryPathAsync(int workspaceId);
    }
}
