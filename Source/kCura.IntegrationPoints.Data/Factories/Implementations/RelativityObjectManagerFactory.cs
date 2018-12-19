using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Facades.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RelativityObjectManagerFactory : IRelativityObjectManagerFactory
	{
		private readonly IHelper _helper;
		private readonly IAPILog _logger;
		private readonly ISecretCatalogFactory _secretCatalogFactory;
		private readonly ISecretManagerFactory _secretManagerFactory;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public RelativityObjectManagerFactory(IHelper helper)
			: this(
				helper, 
				new DefaultSecretCatalogFactory(), 
				new SecretManagerFactory(), 
				instrumentationProvider: null)
		{
			_instrumentationProvider = new ExternalServiceInstrumentationProviderWithoutJobContext(_logger);
		}

		public RelativityObjectManagerFactory(
			IHelper helper,
			ISecretCatalogFactory secretCatalogFactory,
			ISecretManagerFactory secretManagerFactory,
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger();
			_secretCatalogFactory = secretCatalogFactory;
			_secretManagerFactory = secretManagerFactory;
			_instrumentationProvider = instrumentationProvider;
		}

		public IRelativityObjectManager CreateRelativityObjectManager(int workspaceId)
		{
			IServicesMgr servicesMgr = _helper.GetServicesManager();
			return CreateRelativityObjectManager(workspaceId, servicesMgr);
		}

		public IRelativityObjectManager CreateRelativityObjectManager(int workspaceId, IServicesMgr servicesMgr)
		{
			ISecretManager secretManager = _secretManagerFactory?.Create(workspaceId) ?? new SecretManager(workspaceId);
			var secretStoreHelper = new SecretStoreHelper(workspaceId, _helper, secretManager, _secretCatalogFactory);
			var retryHandlerFactory = new RetryHandlerFactory(_logger);
			var objectManagerFacadeFactory = new ObjectManagerFacadeFactory(servicesMgr, _logger, _instrumentationProvider, retryHandlerFactory);

			return new RelativityObjectManager(workspaceId, _logger, secretStoreHelper, objectManagerFacadeFactory);
		}
	}
}
