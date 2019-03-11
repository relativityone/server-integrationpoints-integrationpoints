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

		public T WrapKeplerService<T>(T keplerService)
		{
			KeplerServiceInterceptor dynamicProxy = new KeplerServiceInterceptor(_syncMetrics, _stopwatch, _logger);
			return SexyProxy.Proxy.CreateProxy(keplerService, dynamicProxy.InvocationHandler);
		}
	}
}