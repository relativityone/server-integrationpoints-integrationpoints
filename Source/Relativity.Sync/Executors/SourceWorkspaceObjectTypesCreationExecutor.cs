using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Executors
{
	internal sealed class SourceWorkspaceObjectTypesCreationExecutor : IExecutor<ISourceWorkspaceObjectTypesCreationConfiguration>
	{
		private readonly IRdoManager _rdoManager;
		private readonly ISyncLog _logger;

		public SourceWorkspaceObjectTypesCreationExecutor(IRdoManager rdoManager, ISyncLog logger)
		{
			_rdoManager = rdoManager;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISourceWorkspaceObjectTypesCreationConfiguration configuration, CompositeCancellationToken token)
		{
			try
			{
				await _rdoManager.EnsureTypeExistsAsync<SyncBatchRdo>(configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);
				await _rdoManager.EnsureTypeExistsAsync<SyncProgressRdo>(configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);
				return ExecutionResult.Success();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to create object types in source workspace artifact ID: {sourceWorkspaceArtifactId}", configuration.SourceWorkspaceArtifactId);
				return ExecutionResult.Failure(ex);
			}
		}
	}
}