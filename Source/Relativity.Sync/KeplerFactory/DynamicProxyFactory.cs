using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class DynamicProxyFactory : IDynamicProxyFactory
	{
		private readonly ISyncMetrics _syncMetrics;
		private readonly IStopwatch _stopwatch;
		private readonly ISyncLog _logger;

		public DynamicProxyFactory(ISyncMetrics syncMetrics, IStopwatch stopwatch, ISyncLog logger)
		{
			_syncMetrics = syncMetrics;
			_stopwatch = stopwatch;
			_logger = logger;
		}

		public T WrapKeplerService<T>(T keplerService, Func<Task<T>> keplerServiceFactory) where T : class
		{
			KeplerServiceInterceptor<T> interceptor = new KeplerServiceInterceptor<T>(_syncMetrics, _stopwatch, keplerServiceFactory, _logger);
			ProxyGenerator proxyGenerator = new ProxyGenerator();
			T proxy = proxyGenerator.CreateInterfaceProxyWithTargetInterface<T>(keplerService, interceptor);
			return proxy;
		}
	}
}