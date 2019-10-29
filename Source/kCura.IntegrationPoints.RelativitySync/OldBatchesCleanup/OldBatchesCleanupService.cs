using System;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
	internal class OldBatchesCleanupService : IOldBatchesCleanupService
	{
		private const int OLD_BATCH_DAYS_AMOUNT = 7;
		private readonly IBatchRepository _batchRepository;

		public OldBatchesCleanupService(IBatchRepository batchRepository)
		{
			_batchRepository = batchRepository;
		}

		public async Task DeleteOldBatchesInWorkspaceAsync(int workspaceArtifactId)
		{
			await _batchRepository.DeleteAllOlderThanAsync(workspaceArtifactId, TimeSpan.FromDays(OLD_BATCH_DAYS_AMOUNT)).ConfigureAwait(false);
		}
	}
}