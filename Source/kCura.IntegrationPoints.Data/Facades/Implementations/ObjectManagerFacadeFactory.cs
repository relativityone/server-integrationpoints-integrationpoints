using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;
using Relativity.Services.Objects;
using System;
using kCura.IntegrationPoints.Data.Interfaces;

namespace kCura.IntegrationPoints.Data.Facades.Implementations
{
	internal class ObjectManagerFacadeFactory : IObjectManagerFacadeFactory
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IAPILog _logger;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly IRetryHandlerFactory _retryHandlerFactory;

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
		}

		public IObjectManagerFacade Create(ExecutionIdentity executionIdentity)
		{
			Func<IObjectManager> objectManagerFactory = () => _servicesMgr.CreateProxy<IObjectManager>(executionIdentity);
			var objectManagerFacade = new ObjectManagerFacade(objectManagerFactory, _instrumentationProvider, _logger);
			return new ObjectManagerFacadeWithRetries(objectManagerFacade, _retryHandlerFactory);
		}
	}
}
