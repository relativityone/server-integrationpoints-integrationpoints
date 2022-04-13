using Relativity.API;
using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class JobCleanupExecutor : IExecutor<IJobCleanupConfiguration>
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IAPILog _logger;

		public JobCleanupExecutor(IBatchRepository batchRepository, IAPILog logger)
		{
			_batchRepository = batchRepository;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IJobCleanupConfiguration configuration, CompositeCancellationToken token)
		{
			try
			{
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
