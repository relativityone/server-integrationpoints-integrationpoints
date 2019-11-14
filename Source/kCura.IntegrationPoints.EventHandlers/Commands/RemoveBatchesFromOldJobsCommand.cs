using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class RemoveBatchesFromOldJobsCommand : IEHCommand
	{
		private readonly IEHContext _context;
		private readonly IOldBatchesCleanupService _batchesCleanupService;

		public RemoveBatchesFromOldJobsCommand(IEHContext context, IOldBatchesCleanupService batchesCleanupService)
		{
			_context = context;
			_batchesCleanupService = batchesCleanupService;
		}

		public void Execute()
		{
			int workspaceArtifactId = _context.Helper.GetActiveCaseID();
			_batchesCleanupService.TryToDeleteOldBatchesInWorkspaceAsync(workspaceArtifactId).GetAwaiter().GetResult();
		}
	}
}