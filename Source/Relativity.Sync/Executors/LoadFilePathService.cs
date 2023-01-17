using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class LoadFilePathService : ILoadFilePathService
    {
        private readonly IFileShareService _fileShareService;
        private readonly IAPILog _logger;

        public LoadFilePathService(IFileShareService fileShareService, IAPILog logger)
        {
            _fileShareService = fileShareService;
            _logger = logger;
        }

        public async Task<string> GetJobDirectoryPathAsync(int destinationWorkspaceId, Guid exportRunId)
        {
            string fileSharePath = await _fileShareService.GetWorkspaceFileShareLocationAsync(destinationWorkspaceId)
                .ConfigureAwait(false);

            if (!Directory.Exists(fileSharePath))
            {
                throw new DirectoryNotFoundException($"Workspace fileshare directory: {fileSharePath} does not exist!");
            }

            string jobDirectoryPath = Path.Combine(fileSharePath, "Sync", exportRunId.ToString());

            _logger.LogInformation("Job Directory Path successfully generated: {jobDirectoryPath}", jobDirectoryPath);
            return jobDirectoryPath;
        }
    }
}
