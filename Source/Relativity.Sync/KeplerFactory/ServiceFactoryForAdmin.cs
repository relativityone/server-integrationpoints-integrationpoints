using System;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class ServiceFactoryForAdmin : ISourceServiceFactoryForAdmin, IDestinationServiceFactoryForAdmin
	{
		private readonly ISyncServiceManager _servicesMgr;
		private readonly IDynamicProxyFactory _proxyFactory;

		public ServiceFactoryForAdmin(ISyncServiceManager servicesMgr, IDynamicProxyFactory proxyFactory)
		{
			_servicesMgr = servicesMgr;
			_proxyFactory = proxyFactory;
		}

		public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
		{
			Task<T> KeplerServiceFactory() => Task.FromResult(_servicesMgr.CreateProxy<T>(ExecutionIdentity.System));
			T keplerService = await KeplerServiceFactory().ConfigureAwait(false);
			return _proxyFactory.WrapKeplerService(keplerService, KeplerServiceFactory);
		}
	}
}