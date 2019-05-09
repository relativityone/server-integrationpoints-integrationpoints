using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class SynchronizationExecutor : IExecutor<ISynchronizationConfiguration>
	{
		private readonly IImportJobFactory _importJobFactory;
		private readonly IBatchRepository _batchRepository;
		private readonly ISyncLog _logger;

		public SynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository, ISyncLog logger)
		{
			_importJobFactory = importJobFactory;
			_batchRepository = batchRepository;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Starting import job");

			try
			{
				List<int> batchesIds = (await _batchRepository.GetAllNewBatchesIdsAsync(configuration.SourceWorkspaceArtifactId).ConfigureAwait(false)).ToList();

				foreach (int batchId in batchesIds)
				{
					IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
					IImportJob importJob = _importJobFactory.CreateImportJob(configuration, batch);
					await importJob.RunAsync(token).ConfigureAwait(false);
				}
			}
			catch (SyncException ex)
			{
				_logger.LogError(ex, "Exception occurred while executing import job.");
				return ExecutionResult.Failure("Import job failed.", ex);
			}

			return ExecutionResult.Success();
		}
	}
}