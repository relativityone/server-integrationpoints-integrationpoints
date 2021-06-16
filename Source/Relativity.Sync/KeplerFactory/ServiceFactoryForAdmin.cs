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
            while (true)
            {
                retriesCounter++;
                try
                {
                    return await GetKeplerServiceWrapperAsync<T>(ExecutionIdentity.System).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (retriesCounter >= retriesLimit)
                    {
                        throw ex;
                    }

                    Thread.Sleep(50);
                }

            }
        }

        private async Task<T> GetKeplerServiceWrapperAsync<T>(ExecutionIdentity executionIdentity) where T : class, IDisposable
        {
            Task<T> KeplerServiceFactory() => Task.FromResult(_servicesMgr.CreateProxy<T>(executionIdentity));
            T keplerService = await KeplerServiceFactory().ConfigureAwait(false);
            return _proxyFactory.WrapKeplerService(keplerService, KeplerServiceFactory);
        }


    }
}