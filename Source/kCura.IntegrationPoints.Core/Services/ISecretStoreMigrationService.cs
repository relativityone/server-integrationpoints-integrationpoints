namespace kCura.IntegrationPoints.Core.Services
{
    public interface ISecretStoreMigrationService
    {
        bool TryMigrateSecret(int workspaceID, int integrationPointID, string sourceSecretCatalogID);
    }
}
