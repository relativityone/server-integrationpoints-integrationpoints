using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Helpers
{
	public class CreateTenantIdForSecretStore : ICreateTenantIdForSecretStore
	{
		private readonly IEHContext _context;
		private readonly ISecretCatalogFactory _secretCatalogFactory;
		private readonly ISecretManagerFactory _secretManagerFactory;

		public CreateTenantIdForSecretStore(IEHContext context, ISecretCatalogFactory secretCatalogFactory, ISecretManagerFactory secretManagerFactory)
		{
			_context = context;
			_secretCatalogFactory = secretCatalogFactory;
			_secretManagerFactory = secretManagerFactory;
		}

		public void Create()
		{
			var secretCatalog = _secretCatalogFactory.Create(_context.Helper.GetActiveCaseID());
			var secretManager = _secretManagerFactory.Create(_context.Helper.GetActiveCaseID());
			var tenantId = secretManager.GetTenantID();
			secretCatalog.CreateTenantEncryptionSecret(tenantId);
		}
	}
}