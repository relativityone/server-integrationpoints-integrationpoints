using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Implementations
{
	public class SecretStoreCleanUp : ISecretStoreCleanUp
	{
		private readonly ISecretManager _secretManager;
		private readonly ISecretCatalog _secretCatalog;

		public SecretStoreCleanUp(ISecretManager secretManager, ISecretCatalog secretCatalog)
		{
			_secretManager = secretManager;
			_secretCatalog = secretCatalog;
		}

		public void CleanUpSecretStore()
		{
			RemoveSecrets();
			RemoveTenant();
		}

		private void RemoveSecrets()
		{
			string tenantId = _secretManager.GetTenantID();
			List<Dictionary<string, Dictionary<string, string>>> secrets = _secretCatalog.GetTenantSecrets(tenantId);

			foreach (var secretId in secrets.SelectMany(x => x.Keys))
			{
				SecretRef secret = new SecretRef
				{
					SecretID = secretId,
					TenantID = tenantId
				};
				_secretCatalog.RevokeSecretAsync(secret).Wait();
			}
		}

		private void RemoveTenant()
		{
			string tenantId = _secretManager.GetTenantID();
			//This is a little bit hacky, as we currently don't have better approach for revoking tenants
			SecretRef secret = new SecretRef
			{
				SecretID = $"tenant{tenantId}encryptionkey",
				TenantID = tenantId
			};
			_secretCatalog.RevokeSecretAsync(secret).Wait();
		}
	}
}