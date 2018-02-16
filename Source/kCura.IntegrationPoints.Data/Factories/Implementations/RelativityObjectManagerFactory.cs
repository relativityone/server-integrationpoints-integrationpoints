using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RelativityObjectManagerFactory : IRelativityObjectManagerFactory
	{
		private readonly IHelper _helper;
		private readonly ISecretCatalogFactory _secretCatalogFactory;
		private readonly ISecretManagerFactory _secretManagerFactory;

		public RelativityObjectManagerFactory(IHelper helper, ISecretCatalogFactory secretCatalogFactory, ISecretManagerFactory secretManagerFactory)
		{
			_helper = helper;
			_secretCatalogFactory = secretCatalogFactory;
			_secretManagerFactory = secretManagerFactory;
		}

		public IRelativityObjectManager CreateRelativityObjectManager(int workspaceId)
		{
			var secretManager = _secretManagerFactory.Create(workspaceId);
			return new RelativityObjectManager(workspaceId, _helper,
				new SecretStoreHelper(workspaceId, _helper, secretManager, _secretCatalogFactory));
		}
	}
}
