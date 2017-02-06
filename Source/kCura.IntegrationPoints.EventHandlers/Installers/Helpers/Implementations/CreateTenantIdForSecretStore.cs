using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Implementations
{
	public class CreateTenantIdForSecretStore : ICreateTenantIdForSecretStore
	{
		private readonly ISecretCatalog _secretCatalog;
		private readonly ISecretManager _secretManager;

		public CreateTenantIdForSecretStore(ISecretCatalog secretCatalog, ISecretManager secretManager)
		{
			_secretCatalog = secretCatalog;
			_secretManager = secretManager;
		}

		public void Create()
		{
			var tenantId = _secretManager.GetTenantID();
			_secretCatalog.CreateTenantEncryptionSecret(tenantId);
		}
	}
}