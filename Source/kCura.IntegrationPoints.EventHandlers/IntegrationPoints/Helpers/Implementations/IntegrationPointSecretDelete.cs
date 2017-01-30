using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class IntegrationPointSecretDelete : IIntegrationPointSecretDelete
	{
		private readonly IGenericLibrary<Data.IntegrationPoint> _library;
		private readonly ISecretManager _secretManager;
		private readonly ISecretCatalog _secretCatalog;

		public IntegrationPointSecretDelete(ISecretManager secretManager, ISecretCatalog secretCatalog, IGenericLibrary<Data.IntegrationPoint> library)
		{
			_secretManager = secretManager;
			_secretCatalog = secretCatalog;
			_library = library;
		}

		public void DeleteSecret(int integrationPointId)
		{
			var integrationPointSecret = RetrieveSecretId(integrationPointId);
			//Old IntegrationPoints don't contain SecuredConfiguration
			if (!string.IsNullOrWhiteSpace(integrationPointSecret))
			{
				DeleteSecret(integrationPointSecret);
			}
		}

		private string RetrieveSecretId(int integrationPointId)
		{
			return _library.Read(integrationPointId).SecuredConfiguration;
		}

		private void DeleteSecret(string integrationPointSecret)
		{
			var secretIdentifier = _secretManager.RetrieveIdentifier(integrationPointSecret);
			_secretCatalog.RevokeSecret(secretIdentifier);
		}
	}
}