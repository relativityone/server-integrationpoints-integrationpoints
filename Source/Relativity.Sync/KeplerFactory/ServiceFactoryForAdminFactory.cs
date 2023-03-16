using System;
using Relativity.API;
using Relativity.Sync.Utils;

namespace Relativity.Sync.KeplerFactory
{
    internal class ServiceFactoryForAdminFactory
    {
        private readonly IServicesMgr _serviceManager;
        private readonly IAPILog _logger;
        private readonly Func<IStopwatch> _stopwatch;

        internal ServiceFactoryForAdminFactory(IServicesMgr serviceManager, IAPILog logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
            _stopwatch = () => new StopwatchWrapper();
        }

        internal ISourceServiceFactoryForAdmin Create()
        {
            IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactory(_stopwatch, _logger);
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = new ServiceFactoryForAdmin(_serviceManager, dynamicProxyFactory, _logger);

            return serviceFactoryForAdmin;
        }
    }
}
