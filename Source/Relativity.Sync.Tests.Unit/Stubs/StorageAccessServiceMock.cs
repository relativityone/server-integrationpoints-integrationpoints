using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Storage;
using Relativity.Sync.Transfer.ADLS;

namespace Relativity.Sync.Tests.Unit.Stubs
{
    internal class StorageAccessServiceMock : IStorageAccessService
    {
        public Task<StorageEndpoint[]> GetStorageEndpointsAsync() => Task.FromResult(new StorageEndpoint[0]);

        public Task<bool> DirectoryExistsAsync(string path)
        {
            return Task.FromResult(Directory.Exists(path));
        }

        public Task<DeleteDirectoryResult> DeleteDirectoryAsync(string path, DeleteDirectoryOptions deleteDirectoryOptions = null)
        {
            Directory.Delete(path, deleteDirectoryOptions?.Recursive ?? true);

            return Task.FromResult(DeleteDirectoryResult.Success);
        }

        public Task<Stream> OpenFileAsync(string path, OpenBehavior openBehavior, ReadWriteMode readWriteMode, OpenFileOptions openFileOptions = null)
        {
            var fileInfo = new FileInfo(path);

            fileInfo.Directory.Create();

            fileInfo.Create().Dispose();

            Stream stream = File.Open(path, FileMode.OpenOrCreate);

            return Task.FromResult(stream);
        }

        public Task CopyFileAsync(string sourcePath, string destinationPath, CopyFileOptions copyFileOptions = null, CancellationToken cancellationToken = default)
        {
            File.Copy(sourcePath, destinationPath);

            return Task.CompletedTask;
        }

        public Task DeleteFileAsync(string path, bool force, CancellationToken cancellationToken = default)
        {
            File.Delete(path);

            return Task.CompletedTask;
        }

        public Task WriteAllTextAsync(string path, string contents, WriteAllTextOptions writeAllTextOptions = null)
        {
            File.WriteAllText(path, contents);

            return Task.CompletedTask;
        }
    }
}
