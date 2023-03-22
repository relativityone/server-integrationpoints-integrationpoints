using System.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using Relativity.Storage;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare
{
    public class FakeRelativityStorageService : IRelativityStorageService
    {
        private readonly IStorageAccess<string> _storageAccess = new FakeStorageAccess();

        public Task<IStorageAccess<string>> GetStorageAccessAsync()
        {
            return Task.FromResult(_storageAccess);
        }

        public Task<StorageStream> CreateFileOrTruncateExistingAsync(string path)
        {
            return Task.FromResult<StorageStream>(new FakeStream());
        }

        public Task<string> GetWorkspaceDirectoryPathAsync(int workspaceId)
        {
            return Task.FromResult("DirectoryPath");
        }

        private class FakeStream : StorageStream
        {
            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return 0;
            }

            public override void SetLength(long value)
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }

            public override bool CanWrite => true;

            public override bool CanRead { get; }

            public override bool CanSeek { get; }

            public override long Length { get; }

            public override long Position { get; set; }

            public override string StoragePath { get; }

            public override StorageInterface StorageInterface { get; }
        }

    }
}
