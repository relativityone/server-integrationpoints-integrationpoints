using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.Data.SecretStore
{
	public interface ISecretCatalogFactory
	{
		ISecretCatalog Create(int workspaceArtifactId);
	}
}