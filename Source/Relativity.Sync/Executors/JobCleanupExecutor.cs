using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal sealed class JobCleanupExecutor : IExecutor<IJobCleanupConfiguration>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IIAPIv2RunChecker _iapi2RunChecker;
        private readonly ILoadFilePathService _pathService;
        private readonly IAPILog _logger;

        public JobCleanupExecutor(
            IBatchRepository batchRepository,
            IIAPIv2RunChecker iapi2RunChecker,
            ILoadFilePathService pathService,
            IAPILog logger)
        {
            _batchRepository = batchRepository;
            _iapi2RunChecker = iapi2RunChecker;
            _pathService = pathService;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IJobCleanupConfiguration configuration, CompositeCancellationToken token)
        {
            try
            {
                if (token.IsDrainStopRequested)
                {
                    return ExecutionResult.Success();
                }

                if (_iapi2RunChecker.ShouldBeUsed())
                {
                    _logger.LogInformation("Removing job directory for ExportRunId: {exportRunId}", configuration.ExportRunId);
                    string jobDirectoryPath = await _pathService.GetJobDirectoryPathAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);
                    if (jobDirectoryPath != null && Directory.Exists(jobDirectoryPath))
                    {
                        Directory.Delete(jobDirectoryPath, true);
                    }

                    _logger.LogInformation("Job directory {jobDirectory} successfully removed", jobDirectoryPath);
                }

                await _batchRepository.DeleteAllForConfigurationAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
                return ExecutionResult.Success();
            }
            catch (Exception ex)
            {
                string message = $"There was an error while deleting batches belonging to Sync configuration " +
                    $"ArtifactID: {configuration.SyncConfigurationArtifactId}.";
                _logger.LogError(ex, message);
                ExecutionResult result = ExecutionResult.Failure(message, ex);
                return result;
            }
        }
    }
}
