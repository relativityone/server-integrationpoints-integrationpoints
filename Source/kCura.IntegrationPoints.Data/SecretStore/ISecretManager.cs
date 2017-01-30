using System.Collections;
using System.Collections.Generic;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.Data.SecretStore
{
	public interface ISecretManager
	{
		SecretRef GenerateIdentifier();

		SecretRef RetrieveIdentifier(IntegrationPoint rdo);

		SecretRef RetrieveIdentifier(string integrationPointSecret);

		string RetrieveValue(Dictionary<string, string> dictionary);

		Dictionary<string, string> CreateSecretData(IntegrationPoint rdo);
	}
}