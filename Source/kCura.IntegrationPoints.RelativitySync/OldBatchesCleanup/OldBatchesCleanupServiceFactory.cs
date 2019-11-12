using System;
using kCura.IntegrationPoints.Core.Services;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
	public class OldBatchesCleanupServiceFactory : IOldBatchesCleanupServiceFactory
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly Lazy<IErrorService> _errorService;
		private readonly IAPILog _apiLog;

		public OldBatchesCleanupServiceFactory(IHelper helper, Lazy<IErrorService> errorService, IAPILog apiLog)
		{
			_servicesMgr = helper.GetServicesManager();
			_errorService = errorService;
			_apiLog = apiLog;
		}

		public IOldBatchesCleanupService Create()
		{
			// IBatchRepository is an internal interface from `Relativity.Sync`.
			// `kCura.IntegrationPoints.RelativitySync` project is in an 'Internals visible to' list in `Relativity.Sync` project.
			// In order not to add a new project to the visibility list, we have created a wrapper for needed functionality (`OldBatchesCleanupService`)
			// and a factory that assembles it (`OldBatchesCleanupServiceFactory`).

			ISourceServiceFactoryForAdmin serviceFactory = new SimpleServiceFactoryForAdmin(_servicesMgr);
			IBatchRepository batchRepository = new BatchRepository(serviceFactory, new DateTimeWrapper());

			return new OldBatchesCleanupService(batchRepository, _errorService, _apiLog);
		}
	}
}