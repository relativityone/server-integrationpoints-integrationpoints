using System;
using Relativity.Sync.Utils;

namespace Relativity.Sync.KeplerFactory
{
    internal class ServiceFactoryForAdminFactory
    {
        private readonly ISyncServiceManager _serviceManager;
        private readonly ISyncLog _logger;
        private readonly IRandom _random;
        private readonly Func<IStopwatch> _stopwatch;
        
        internal ServiceFactoryForAdminFactory(ISyncServiceManager serviceManager, ISyncLog logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
            _random = new WrapperForRandom();
            _stopwatch = () => new StopwatchWrapper();
        }

        internal ISourceServiceFactoryForAdmin Create()
        {
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = new ServiceFactoryForAdmin(_serviceManager, new DynamicProxyFactory(
                _stopwatch, _random, _logger), _random, _logger);

            return serviceFactoryForAdmin;
        }
    }
}
