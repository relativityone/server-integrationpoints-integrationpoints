using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ADLS;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
    internal class LoadFilePathService : ILoadFilePathService
    {
        private const int _LONG_TEXT_FOLDER_SEGMENT_SIZE = 2;

        private readonly IFileShareService _fileShareService;
        private readonly ILoadFileConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IStorageAccessService _storageAccessService;
        private readonly IAPILog _logger;

        private readonly CacheItemPolicy _memoryCacheItemPolicy = new CacheItemPolicy();

        public LoadFilePathService(
            IFileShareService fileShareService,
            ILoadFileConfiguration configuration,
            IMemoryCache memoryCache,
            IStorageAccessService storageAccessService,
            IAPILog logger)
        {
            _fileShareService = fileShareService;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _storageAccessService = storageAccessService;
            _logger = logger;
        }

        public async Task<string> GetJobDirectoryPathAsync()
        {
            string cacheKey = GetCacheKey(
                _configuration.DestinationWorkspaceArtifactId,
                _configuration.ExportRunId);

            string jobDirectoryPath = _memoryCache.Get<string>(cacheKey);

            if (string.IsNullOrEmpty(jobDirectoryPath))
            {
                string fileSharePath = await _fileShareService
                    .GetWorkspaceFileShareLocationAsync(_configuration.DestinationWorkspaceArtifactId)
                    .ConfigureAwait(false);

                bool exists = await _storageAccessService.DirectoryExistsAsync(fileSharePath).ConfigureAwait(false);
                if (!exists)
                {
                    throw new DirectoryNotFoundException($"Workspace fileshare directory: {fileSharePath} does not exist!");
                }

                jobDirectoryPath = Path.Combine(fileSharePath, "Sync", _configuration.ExportRunId.ToString());

                _logger.LogInformation("Job Directory Path successfully generated: {jobDirectoryPath}", jobDirectoryPath);

                _memoryCache.Add(cacheKey, jobDirectoryPath, _memoryCacheItemPolicy);
            }

            return jobDirectoryPath;
        }

        public async Task<string> GenerateBatchLoadFilePathAsync(IBatch batch)
        {
            _logger.LogInformation("Preparing LoadFile path for Batch {batchId} - {batchGuid}...", batch.ArtifactId, batch.BatchGuid);
            string batchLoadFilePath;

            string jobDirectoryPath = await GetJobDirectoryPathAsync().ConfigureAwait(false);

            batchLoadFilePath = Path.Combine(jobDirectoryPath, $"{batch.BatchGuid}.dat");

            _logger.LogInformation("LoadFile Path for Batch {batchId} was prepared - {batchPath}", batch.ArtifactId, batchLoadFilePath);

            return batchLoadFilePath;
        }

        public async Task<string> GenerateLongTextFilePathAsync(Guid longTextId)
        {
            string jobDirectoryPath = await GetJobDirectoryPathAsync().ConfigureAwait(false);

            string longTextFileName = $"{longTextId}.txt";

            return Path.Combine(
                jobDirectoryPath,
                "LongTexts",
                longTextFileName.Substring(0, _LONG_TEXT_FOLDER_SEGMENT_SIZE).ToLower(),
                longTextFileName);
        }

        public async Task<string> GetLoadFileRelativeLongTextFilePathAsync(string longTextFilePath)
        {
            string jobDirectoryPath = await GetJobDirectoryPathAsync().ConfigureAwait(false);

            return longTextFilePath.MakeRelativeTo(jobDirectoryPath);
        }

        private string GetCacheKey(int destinationWorkspaceId, Guid exportRunId) => $"{destinationWorkspaceId}:{exportRunId}";
    }
}
