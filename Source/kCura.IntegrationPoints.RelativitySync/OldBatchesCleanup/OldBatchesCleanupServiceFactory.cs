using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.RelativitySync.RipOverride;
using Relativity.API;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

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
            IBatchRepository batchRepository = new BatchRepository(
                new RdoManager(new SyncLog(_apiLog), serviceFactory, new RdoGuidProvider()),
                serviceFactory, new DateTimeWrapper());

            return new OldBatchesCleanupService(batchRepository, _errorService, _apiLog);
        }
    }
}