using System;
using System.Threading;
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
            int retriesCounter = 0;
            const int retriesLimit = 3;
            Exception proxyException;
            do
            {
                retriesCounter++;
                try
                {
                    return await GetKeplerServiceWrapperAsync<T>(ExecutionIdentity.System).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    proxyException =  ex;
                    await Task.Delay(50);
                }

            } while (retriesCounter < retriesLimit);

            throw proxyException;
        }

        private async Task<T> GetKeplerServiceWrapperAsync<T>(ExecutionIdentity executionIdentity) where T : class, IDisposable
        {
            Task<T> KeplerServiceFactory() => Task.FromResult(_servicesMgr.CreateProxy<T>(executionIdentity));
            T keplerService = await KeplerServiceFactory().ConfigureAwait(false);
            return _proxyFactory.WrapKeplerService(keplerService, KeplerServiceFactory);
        }


    }
}