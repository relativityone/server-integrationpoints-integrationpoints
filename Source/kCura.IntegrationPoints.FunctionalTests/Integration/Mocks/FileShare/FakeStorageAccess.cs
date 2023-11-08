﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Storage;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare
{
    public class FakeStorageAccess : IStorageAccess<string>
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Task CopyFileAsync(string sourcePath, string destinationPath, CopyFileOptions copyFileOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task CopyFileSetAsync(IEnumerable<CopyFileEntry> pathSet, CopyFileSetOptions options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<CreateDirectoryResult> CreateDirectoryAsync(string path, CreateDirectoryOptions createDirectoryOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(CreateDirectoryResult.Success);
        }

        public Task<DeleteDirectoryResult> DeleteDirectoryAsync(string path, DeleteDirectoryOptions deleteDirectoryOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(DeleteDirectoryResult.Success);
        }

        public Task<DeleteFileResult> DeleteFileAsync(string path, DeleteFileOptions deleteFileOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<DirectoryEntry> EnumerateDirectoryAsync(string path, EnumerateDirectoryOptions enumerateDirectoryOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<DirectoryEntry[]> EnumerateDirectoryPagedAsync(
            string path,
            EnumerateDirectoryPagedOptions enumerateDirectoryPagedOptions = null,
            CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<StorageStream> OpenFileAsync(string path, OpenBehavior openBehavior, ReadWriteMode readWriteMode,
            OpenFileOptions openFileOptions = null, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task MoveFileAsync(string sourcePath, string destinationPath, MoveFileOptions moveFileOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task MoveDirectoryAsync(string sourcePath, string destinationPath, MoveDirectoryOptions moveDirectoryOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<byte[]> ReadAllBytesAsync(
            string path,
            ReadAllBytesOptions readAllBytesOptions = null,
            CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> ReadAllTextAsync(
            string path,
            ReadAllTextOptions readAllTextOptions = null,
            CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<byte[]> ReadByteRangeAsync(string path, long offset, int length, ReadByteRangeOptions readByteRangeOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<int> ReadByteRangeToAsync(string path, long offset, int length, Stream destinationStream,
            ReadByteRangeToOptions readByteRangeToOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task RenameFileAsync(string sourcePath, string destinationPath, RenameFileOptions renameFileOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task RenameDirectoryAsync(string sourcePath, string destinationPath,
            RenameDirectoryOptions renameDirectoryOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task UploadFileAsync(string path, Stream content, UploadFileOptions uploadFileOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<FileMetadata> GetFileMetadataAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<DirectoryMetadata> GetDirectoryMetadataAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<CopyDirectoryResult> CopyDirectoryAsync(string sourcePath, string destinationPath, CopyDirectoryOptions copyDirectoryOptions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> FileSystemEntryExistsAsync(string path, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task EnsureEndpointConnectedAsync(string path, EnsureEndpointConnectedOptions ensureEndpointConnectedOptions = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public StorageInterface GetResolvedPathStorageInterface(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task ReadAllBytesToAsync(string path, Stream destinationStream, ReadAllBytesToOptions readAllBytesToOptions = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task ReadAllTextToAsync(string path, TextWriter textWriter, ReadAllTextToOptions readAllTextToOptions = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public StorageAccessKind StorageAccessKind { get; }
    }
}
