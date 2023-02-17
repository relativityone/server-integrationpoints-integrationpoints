using System;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;
using Relativity.Services.Objects;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation
{
    internal class ObjectManagerFacadeFactory : IObjectManagerFacadeFactory
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly Func<IObjectManagerFacade, IObjectManagerFacade>[] _decorators;

        public ObjectManagerFacadeFactory(
            IServicesMgr servicesMgr,
            IAPILog logger,
            IExternalServiceInstrumentationProvider instrumentationProvider,
            IRetryHandlerFactory retryHandlerFactory)
        {
            _servicesMgr = servicesMgr;

            _decorators = new Func<IObjectManagerFacade, IObjectManagerFacade>[]
            {
                (om) => new ObjectManagerFacadeInstrumentationDecorator(om,
                        instrumentationProvider,
                        logger),
                (om) => new ObjectManagerFacadeRetryDecorator(om, retryHandlerFactory),
                (om) => new ObjectManagerFacadeDiscoverHeavyRequestDecorator(om, logger)
            };
        }

        public IObjectManagerFacade Create(ExecutionIdentity executionIdentity)
        {
            Func<IObjectManager> objectManagerFactory =
                () => _servicesMgr.CreateProxy<IObjectManager>(executionIdentity);
            IObjectManagerFacade objectManagerFacade = new ObjectManagerFacade(objectManagerFactory);
            return _decorators.Aggregate(
                objectManagerFacade,
                (objectManager, decorator) => decorator(objectManager));
        }
    }
}
