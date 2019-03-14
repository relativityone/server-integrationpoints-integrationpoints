using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class IntegrationPointSecretDelete : IIntegrationPointSecretDelete
	{
		private readonly IIntegrationPointRepository _integrationPointRepository;
		private readonly ISecretManager _secretManager;
		private readonly ISecretCatalog _secretCatalog;

		public IntegrationPointSecretDelete(ISecretManager secretManager, ISecretCatalog secretCatalog, IIntegrationPointRepository integrationPointRepository)
		{
			_secretManager = secretManager;
			_secretCatalog = secretCatalog;
			_integrationPointRepository = integrationPointRepository;
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
			return _integrationPointRepository.GetSecuredConfiguration(integrationPointId);
		}

		private void DeleteSecret(string integrationPointSecret)
		{
			var secretIdentifier = _secretManager.RetrieveIdentifier(integrationPointSecret);
			_secretCatalog.RevokeSecret(secretIdentifier);
		}
	}
}