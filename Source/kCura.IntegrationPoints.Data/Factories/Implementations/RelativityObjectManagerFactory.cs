using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
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

		public RelativityObjectManagerFactory(IHelper helper, ISecretCatalogFactory secretCatalogFactory,
			ISecretManagerFactory secretManagerFactory, IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger();
			_secretCatalogFactory = secretCatalogFactory;
			_secretManagerFactory = secretManagerFactory;
			_instrumentationProvider = instrumentationProvider;
		}
		
		public RelativityObjectManagerFactory(IHelper helper)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger();
			_secretCatalogFactory = new DefaultSecretCatalogFactory();
			_secretManagerFactory = new SecretManagerFactory();
			_instrumentationProvider = new ExternalServiceInstrumentationProviderWithoutJobContext(_logger);
		}

		public IRelativityObjectManager CreateRelativityObjectManager(int workspaceId)
		{
			IServicesMgr servicesMgr = _helper.GetServicesManager();
			return CreateRelativityObjectManager(workspaceId, servicesMgr);
		}

		public IRelativityObjectManager CreateRelativityObjectManager(int workspaceId, IServicesMgr servicesMgr)
		{
			ISecretManager secretManager = _secretManagerFactory?.Create(workspaceId) ?? new SecretManager(workspaceId);
			ISecretStoreHelper secretStoreHelper =
				new SecretStoreHelper(workspaceId, _helper, secretManager, _secretCatalogFactory);
			return new RelativityObjectManager(
				workspaceId, servicesMgr, _logger, secretStoreHelper, _instrumentationProvider);
		}
	}
}
