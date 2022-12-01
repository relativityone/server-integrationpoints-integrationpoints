using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.API;
using Relativity.Sync.Utils;

namespace Relativity.Sync.KeplerFactory
{
    internal sealed class DynamicProxyFactory : IDynamicProxyFactory
    {
        private readonly Func<IStopwatch> _stopwatch;
        private readonly IRandom _random;
        private readonly IAPILog _logger;

        // If you have a long running process and you have to create many dynamic proxies, you should make sure to reuse the same ProxyGenerator instance.
        // If not, be aware that you will then bypass the caching mechanism. Side effects are high CPU usage and constant increase in memory consumption.
        // https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md
        // We also have to set disableSignedModule to true to prevent Castle from signing the dynamic proxy dll which can lead to FileLoadException
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator(true);

        public DynamicProxyFactory(Func<IStopwatch> stopwatch, IRandom random, IAPILog logger)
        {
            _stopwatch = stopwatch;
            _random = random;
            _logger = logger;
        }

        public T WrapKeplerService<T>(T keplerService, Func<Task<T>> keplerServiceFactory) where T : class
        {
            KeplerServiceInterceptor<T> interceptor = new KeplerServiceInterceptor<T>(_stopwatch, keplerServiceFactory, _random, _logger);

            return _proxyGenerator.CreateInterfaceProxyWithTargetInterface<T>(keplerService, interceptor);
        }
    }
}
