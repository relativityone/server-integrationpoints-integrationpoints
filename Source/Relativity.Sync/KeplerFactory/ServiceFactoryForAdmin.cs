using System;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class ServiceFactoryForAdmin : ISourceServiceFactoryForAdmin, IDestinationServiceFactoryForAdmin
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IDynamicProxyFactory _proxyFactory;

		public ServiceFactoryForAdmin(IServicesMgr servicesMgr, IDynamicProxyFactory proxyFactory)
		{
			_servicesMgr = servicesMgr;
			_proxyFactory = proxyFactory;
		}

		public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
		{
			await Task.Yield();
			T KeplerServiceFactory() => _servicesMgr.CreateProxy<T>(ExecutionIdentity.System);
			T keplerService = KeplerServiceFactory();
			return _proxyFactory.WrapKeplerService(keplerService, KeplerServiceFactory);
		}
	}
}