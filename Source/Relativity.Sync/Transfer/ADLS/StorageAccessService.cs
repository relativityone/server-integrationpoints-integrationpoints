using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer.ADLS
{
    internal class StorageAccessService : IStorageAccessService
    {
        private const string _ADLER_SIEBEN_PTCI_ID = "PTCI-2456712";
        private const string _RELATIVITY_SYNC_SERVICE_NAME = "relativity-sync";

        private readonly ApplicationDetails _applicationDetails = new ApplicationDetails(_ADLER_SIEBEN_PTCI_ID, _RELATIVITY_SYNC_SERVICE_NAME);

        private readonly IHelper _helper;
        private readonly Func<IStopwatch> _stopwatch;
        private readonly IAPILog _logger;

        private IStorageAccess<string> _storageAccess;
        private StorageEndpoint[] _storageEndpoints;

        public StorageAccessService(IHelper helper, Func<IStopwatch> stopwatch, IAPILog logger)
        {
            _helper = helper;
            _stopwatch = stopwatch;
            _logger = logger;
        }

        public async Task<StorageEndpoint[]> GetStorageEndpointsAsync() => _storageEndpoints ??= await GetStorageEndpointsInternalAsync().ConfigureAwait(false);

        public async Task<bool> DirectoryExistsAsync(string path)
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            return await storageAccess.DirectoryExistsAsync(path).ConfigureAwait(false);
        }

        public async Task<DeleteDirectoryResult> DeleteDirectoryAsync(string path, DeleteDirectoryOptions deleteDirectoryOptions = null)
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            return await storageAccess.DeleteDirectoryAsync(path, deleteDirectoryOptions).ConfigureAwait(false);
        }

        public async Task<Stream> OpenFileAsync(string path, OpenBehavior openBehavior, ReadWriteMode readWriteMode, OpenFileOptions openFileOptions = null)
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            return await storageAccess.OpenFileAsync(path, openBehavior, readWriteMode, openFileOptions).ConfigureAwait(false);
        }

        public async Task CopyFileAsync(string sourcePath, string destinationPath, CopyFileOptions copyFileOptions = null, CancellationToken cancellationToken = default)
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            await storageAccess.CopyFileAsync(sourcePath, destinationPath, copyFileOptions, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteFileAsync(string path, bool force, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting file - Path: {path}...", path);

            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            DeleteFileResult deleteResultObject = await storageAccess.DeleteFileAsync(path, new DeleteFileOptions { Force = force }, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("File deleted - DeleteResult: {deleteResult}, WasCancelled: {wasCancelled}", deleteResultObject, cancellationToken.IsCancellationRequested);
        }

        public async Task WriteAllTextAsync(string path, string contents, WriteAllTextOptions writeAllTextOptions = null)
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            await storageAccess.WriteAllTextAsync(path, contents, writeAllTextOptions).ConfigureAwait(false);
        }

        private async Task<IStorageAccess<string>> GetStorageAccessAsync() => _storageAccess ??= await CreateStorageAccessInternalAsync().ConfigureAwait(false);

        private async Task<IStorageAccess<string>> CreateStorageAccessInternalAsync()
        {
            _logger.LogInformation("Creating StorageAccess...");
            IStopwatch sw = _stopwatch();
            sw.Start();

            IStorageAccess<string> storageAccessor = await _helper
                .GetStorageAccessorAsync(StorageAccessPermissions.GenericReadWrite, _applicationDetails)
                .ConfigureAwait(false);
            if (storageAccessor == null)
            {
                string message = "Storage Accessor not found.";
                _logger.LogError(message);

                throw new NotFoundException(message);
            }

            sw.Stop();

            _logger.LogInformation("StorageAccess was created in {elapsed} seconds.", sw.Elapsed.TotalSeconds);

            return storageAccessor;
        }

        private async Task<StorageEndpoint[]> GetStorageEndpointsInternalAsync()
        {
            _logger.LogInformation("Retrieving StorageEndpoints...");

            IStopwatch sw = _stopwatch();
            sw.Start();
            _storageEndpoints = await _helper.GetStorageEndpointsAsync(_applicationDetails).ConfigureAwait(false);
            sw.Stop();

            _logger.LogInformation("StorageEndpoints retrieved in {elapsed} seconds.", sw.Elapsed.TotalSeconds);

            _logger.LogInformation("Retrieved bedrock server(s): {@bedrockEndpoints}", _storageEndpoints);

            return _storageEndpoints;
        }
    }
}
