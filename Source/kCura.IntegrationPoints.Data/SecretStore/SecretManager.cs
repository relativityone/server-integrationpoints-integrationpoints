using System;
using System.Collections.Generic;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.Data.SecretStore
{
	public class SecretManager : ISecretManager
	{
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

		private string GetTenantID()
		{
			//TODO this is temporary solution for now. we have to create our own tenant id
			return null;
		}
	}
}