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
			ISourceServiceFactoryForAdmin serviceFactory = new ServiceFactoryForAdmin(_servicesMgr);
			IBatchRepository batchRepository = new BatchRepository(serviceFactory, new DateTimeWrapper());

			return new OldBatchesCleanupService(batchRepository, _errorService, _apiLog);
		}
	}
}