using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
	public class OldBatchesCleanupServiceFactory : IOldBatchesCleanupServiceFactory
	{
		private readonly IServicesMgr _servicesMgr;

		public OldBatchesCleanupServiceFactory(IHelper helper)
		{
			_servicesMgr = helper.GetServicesManager();
		}

		public IOldBatchesCleanupService Create()
		{
			// IBatchRepository is an internal interface from `Relativity.Sync`.
			// `kCura.IntegrationPoints.RelativitySync` project is in an 'Internals visible to' list in `Relativity.Sync` project.
			// In order not to add a new project to the visibility list, we have created a wrapper for needed functionality (`OldBatchesCleanupService`)
			// and a factory that assembles it (`OldBatchesCleanupServiceFactory`).

			ISourceServiceFactoryForAdmin serviceFactory = new SimpleServiceFactoryForAdmin(_servicesMgr);
			IBatchRepository batchRepository = new BatchRepository(serviceFactory, new DateTimeWrapper());

			return new OldBatchesCleanupService(batchRepository);
		}
	}
}