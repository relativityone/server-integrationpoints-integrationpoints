using System;
using System.Collections.Generic;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.Data.SecretStore
{
	public class SecretManager : ISecretManager
	{
		private readonly int _workspaceArtifactId;

		public SecretManager(int workspaceArtifactId)
		{
			_workspaceArtifactId = workspaceArtifactId;
		}

		public SecretRef GenerateIdentifier()
		{
			return new SecretRef
			{
				SecretID = Guid.NewGuid().ToString(),
				TenantID = GetTenantID()
			};
		}

		public SecretRef RetrieveIdentifier(IntegrationPoint rdo)
		{
			return RetrieveIdentifier(rdo.SecuredConfiguration);
		}

		public SecretRef RetrieveIdentifier(string integrationPointSecret)
		{
			if (string.IsNullOrWhiteSpace(integrationPointSecret))
			{
				//Assuming that this is an old IntegrationPoint without SecuredConfiguration
				return GenerateIdentifier();
			}
			return new SecretRef
			{
				SecretID = integrationPointSecret,
				TenantID = GetTenantID()
			};
		}

		public string RetrieveValue(Dictionary<string, string> dictionary)
		{
			return dictionary[nameof(IntegrationPoint.SecuredConfiguration)];
		}

		public Dictionary<string, string> CreateSecretData(IntegrationPoint rdo)
		{
			return new Dictionary<string, string> {{nameof(IntegrationPoint.SecuredConfiguration), rdo.SecuredConfiguration}};
		}

		public Dictionary<string, string> CreateSecretData(string securedConfiguration)
		{
			return new Dictionary<string, string> { { nameof(IntegrationPoint.SecuredConfiguration), securedConfiguration } };
		}

		public string GetTenantID()
		{
			return $"{SecretStoreConstants.TENANT_ID_PREFIX}:{_workspaceArtifactId}";
		}
	}
}