﻿using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.Sync.KeplerFactory
{
    internal sealed class ServiceFactoryForAdmin : ServiceFactoryBase, ISourceServiceFactoryForAdmin, IDestinationServiceFactoryForAdmin
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IDynamicProxyFactory _proxyFactory;

        public ServiceFactoryForAdmin(IServicesMgr servicesMgr, IDynamicProxyFactory proxyFactory, IAPILog logger)
            : base(logger)
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
