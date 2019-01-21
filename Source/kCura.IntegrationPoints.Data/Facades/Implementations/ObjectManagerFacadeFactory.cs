using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;
using Relativity.Services.Objects;
using System;
using System.Linq;
using kCura.IntegrationPoints.Data.Interfaces;

namespace kCura.IntegrationPoints.Data.Facades.Implementations
{
	internal class ObjectManagerFacadeFactory : IObjectManagerFacadeFactory
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IAPILog _logger;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly IRetryHandlerFactory _retryHandlerFactory;

		private readonly Func<IObjectManagerFacade, IObjectManagerFacade>[] _decorators;

		public ObjectManagerFacadeFactory(
			IServicesMgr servicesMgr,
			IAPILog logger,
			IExternalServiceInstrumentationProvider instrumentationProvider,
			IRetryHandlerFactory retryHandlerFactory)
		{
			_servicesMgr = servicesMgr;
			_logger = logger;
			_instrumentationProvider = instrumentationProvider;
			_retryHandlerFactory = retryHandlerFactory;

			_decorators = new Func<IObjectManagerFacade, IObjectManagerFacade>[] 
			{
				(om) => new ObjectManagerFacadeInstrumentationDecorator(om,
						_instrumentationProvider,
						_logger),
				(om) => new ObjectManagerFacadeDiscoverHeavyRequestDecorator(om, _logger),
				(om) => new ObjectManagerFacadeRetryDecorator(om, _retryHandlerFactory)
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
