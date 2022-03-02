using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Utils;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class ServiceFactoryForAdmin : ServiceFactoryBase, ISourceServiceFactoryForAdmin, IDestinationServiceFactoryForAdmin
	{
		private readonly ISyncServiceManager _servicesMgr;
		private readonly IDynamicProxyFactory _proxyFactory;

		public ServiceFactoryForAdmin(ISyncServiceManager servicesMgr, IDynamicProxyFactory proxyFactory,
            IRandom random, ISyncLog logger)
		    : base(random, logger)
		{
			_servicesMgr = servicesMgr;
			_proxyFactory = proxyFactory;
		}

        protected override async Task<T> CreateProxyInternalAsync<T>()
        {
            Task<T> KeplerServiceFactory() => Task.FromResult(_servicesMgr.CreateProxy<T>(ExecutionIdentity.System));
            T keplerService = await KeplerServiceFactory().ConfigureAwait(false);
            return _proxyFactory.WrapKeplerService(keplerService, KeplerServiceFactory);
        }
    }
}