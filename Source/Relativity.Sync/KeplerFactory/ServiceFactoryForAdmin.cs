using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class ServiceFactoryForAdmin : ServiceFactoryBase, ISourceServiceFactoryForAdmin, IDestinationServiceFactoryForAdmin
	{
		private readonly ISyncServiceManager _servicesMgr;
		private readonly IDynamicProxyFactory _proxyFactory;

		public ServiceFactoryForAdmin(ISyncServiceManager servicesMgr, IDynamicProxyFactory proxyFactory)
		{
			_servicesMgr = servicesMgr;
			_proxyFactory = proxyFactory;
		}

        internal override async Task<T> CreateProxyInternalAsync<T>()
        {
            Task<T> KeplerServiceFactory() => Task.FromResult(_servicesMgr.CreateProxy<T>(ExecutionIdentity));
            T keplerService = await KeplerServiceFactory().ConfigureAwait(false);
            return _proxyFactory.WrapKeplerService(keplerService, KeplerServiceFactory);
        }
		
    }
}