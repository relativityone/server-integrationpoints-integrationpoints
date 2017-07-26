namespace kCura.IntegrationPoints.Data.SecretStore
{
	public class SecretManagerFactory : ISecretManagerFactory
	{
		public ISecretManager Create(int workspaceId)
		{
			return new SecretManager(workspaceId);
		}
	}
}