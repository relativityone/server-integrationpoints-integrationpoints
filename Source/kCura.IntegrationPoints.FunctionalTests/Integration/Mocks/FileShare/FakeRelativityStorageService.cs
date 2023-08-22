using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Storage;
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

        public Task<IEnumerable<StorageEndpoint>> GetStorageEndpointsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StorageStream> CreateFileOrTruncateExistingAsync(string path)
        {
            var fileInfo = new FileInfo(path);
            fileInfo.Directory.Create();
            Stream stream = File.Open(path, FileMode.OpenOrCreate);
            return Task.FromResult<StorageStream>(new FakeStream(stream, path));
        }

        public Task<StorageStream> OpenFileAsync(OpenFileParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public async Task<IList<string>> ReadAllLinesAsync(string filePath)
        {
            Stream stream = File.OpenRead(filePath);
            List<string> lines = new List<string>();
            using (TextReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        public Task<string> GetWorkspaceDirectoryPathAsync(int workspaceId)
        {
            return Task.FromResult("DirectoryPath");
        }

        public Task<DirectoryInfo> PrepareImportDirectoryAsync(int workspaceId, Guid importJobId)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "RIP", "Tests",
                "CustomProviderImport", workspaceId.ToString(), importJobId.ToString()));
            dir.Create();

            return Task.FromResult(dir);
        }

        public Task DeleteDirectoryRecursiveAsync(string directoryPath)
        {
            // don't delete directory because we want to validate written files

            return Task.CompletedTask;
        }

        private class FakeStream : StorageStream
        {
            private readonly Stream _stream;
            private readonly string _storagePath;

            public FakeStream(Stream stream, string storagePath)
            {
                _stream = stream;
                _storagePath = storagePath;
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override void Close()
            {
                base.Close();
                _stream.Close();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _stream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _stream.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
            }

            public override bool CanWrite => true;

            public override bool CanRead { get; }

            public override bool CanSeek { get; }

            public override long Length { get; }

            public override long Position { get; set; }

            public override string StoragePath => _storagePath;

            public override StorageInterface StorageInterface { get; }
        }

    }
}
