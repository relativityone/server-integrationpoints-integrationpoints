using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
	public interface IOldBatchesCleanupServiceFactory
	{
		IOldBatchesCleanupService Create();
	}
}