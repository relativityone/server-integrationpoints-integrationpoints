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
			ISourceServiceFactoryForAdmin serviceFactory = new SimpleServiceFactoryForAdmin(_servicesMgr);
			IBatchRepository batchRepository = new BatchRepository(serviceFactory, new DateTimeWrapper());

			return new OldBatchesCleanupService(batchRepository);
		}
	}
}