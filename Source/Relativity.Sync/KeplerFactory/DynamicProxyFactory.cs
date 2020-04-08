﻿using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Utils;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class DynamicProxyFactory : IDynamicProxyFactory
	{
		private readonly ISyncMetrics _syncMetrics;
		private readonly Func<IStopwatch> _stopwatch;
		private readonly IRandom _random;
		private readonly ISyncLog _logger;

		// If you have a long running process and you have to create many dynamic proxies, you should make sure to reuse the same ProxyGenerator instance.
		// If not, be aware that you will then bypass the caching mechanism. Side effects are high CPU usage and constant increase in memory consumption.
		// https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md
		private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

		public DynamicProxyFactory(ISyncMetrics syncMetrics, Func<IStopwatch> stopwatch, IRandom random, ISyncLog logger)
		{
			_syncMetrics = syncMetrics;
			_stopwatch = stopwatch;
			_random = random;
			_logger = logger;
		}

		public T WrapKeplerService<T>(T keplerService, Func<Task<T>> keplerServiceFactory) where T : class
		{
			KeplerServiceInterceptor<T> interceptor = new KeplerServiceInterceptor<T>(_syncMetrics, _stopwatch, keplerServiceFactory, _random, _logger);
			
			return _proxyGenerator.CreateInterfaceProxyWithTargetInterface<T>(keplerService, interceptor);
		}
	}
}