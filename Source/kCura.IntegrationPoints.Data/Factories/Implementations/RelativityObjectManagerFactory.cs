using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
    public class RelativityObjectManagerFactory : IRelativityObjectManagerFactory
    {
        private readonly IHelper _helper;
        private readonly IAPILog _logger;
        private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

        public RelativityObjectManagerFactory(IHelper helper)
            : this(
                helper,
                instrumentationProvider: null)
        {
            _instrumentationProvider = new ExternalServiceInstrumentationProviderWithoutJobContext(_logger);
        }

        public RelativityObjectManagerFactory(
            IHelper helper,
            IExternalServiceInstrumentationProvider instrumentationProvider)
        {
            _helper = helper;
            _logger = helper.GetLoggerFactory().GetLogger();
            _instrumentationProvider = instrumentationProvider;
        }

        public IRelativityObjectManager CreateRelativityObjectManager(int workspaceId)
        {
            IServicesMgr servicesMgr = _helper.GetServicesManager();
            return CreateRelativityObjectManager(workspaceId, servicesMgr);
        }

        public IRelativityObjectManager CreateRelativityObjectManager(int workspaceId, IServicesMgr servicesMgr)
        {
            var retryHandlerFactory = new RetryHandlerFactory(_logger);
            var objectManagerFacadeFactory = new ObjectManagerFacadeFactory(
                servicesMgr,
                _logger,
                _instrumentationProvider,
                retryHandlerFactory
            );

            return new RelativityObjectManager(
                workspaceId,
                _logger,
                objectManagerFacadeFactory
            );
        }
    }
}
