namespace kCura.IntegrationPoints.Data.SecretStore
{
	public interface ISecretManagerFactory
	{
		ISecretManager Create(int workspaceId);
	}
}