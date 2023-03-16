using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Storage;

namespace Relativity.Sync.Transfer.ADLS
{
    internal interface IStorageAccessService
    {
        Task<StorageEndpoint[]> GetStorageEndpointsAsync();

        Task<bool> DirectoryExistsAsync(string path);

        Task<DeleteDirectoryResult> DeleteDirectoryAsync(string path, DeleteDirectoryOptions? deleteDirectoryOptions = null);

        Task<Stream> OpenFileAsync(string path, OpenBehavior openBehavior, ReadWriteMode readWriteMode, OpenFileOptions openFileOptions = null);

        Task CopyFileAsync(string sourcePath, string destinationPath, CopyFileOptions copyFileOptions = null, CancellationToken cancellationToken = default);

        Task DeleteFileAsync(string path, bool force, CancellationToken cancellationToken = default);

        Task WriteAllTextAsync(string path, string contents, WriteAllTextOptions? writeAllTextOptions = null);
    }
}
