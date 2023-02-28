using System;
using kCura.IntegrationPoints.Core.Models;
using Relativity.API;
using Exception = kCura.Notification.Exception;

namespace kCura.IntegrationPoints.Core.Services
{
    internal class SecretStoreMigrationService : ISecretStoreMigrationService
    {
        private const string _TENANT_ID_PREFIX = "92080CA4-4903-41B0-9E4C-4DC7DF961A8E";
        private const string _MIGRATION_FAILED_MESSAGE = "Migration of the secret failed";
        private readonly ISecretStoreMigrator _secretStoreMigrator;
        private readonly Lazy<IErrorService> _errorService;
        private readonly IAPILog _apiLog;

        public SecretStoreMigrationService(
            ISecretStoreMigrator secretStoreMigrator,
            Lazy<IErrorService> errorService,
            IAPILog apiLog)
        {
            _secretStoreMigrator = secretStoreMigrator;
            _errorService = errorService;
            _apiLog = apiLog;
        }

        public bool TryMigrateSecret(int workspaceID, int integrationPointID, string sourceSecretCatalogID)
        {
            string sourceSecretCatalogTenantID = $"{_TENANT_ID_PREFIX}:{workspaceID}";
            string destinationSecretPath = $"{workspaceID}/{integrationPointID}/{sourceSecretCatalogID}";

            bool isMigrated = false;
            try
            {
                isMigrated = _secretStoreMigrator.MigrateFromSecretCatalog(
                    sourceSecretCatalogTenantID,
                    sourceSecretCatalogID,
                    destinationSecretPath
                );
            }
            catch (Exception ex)
            {
                _apiLog.LogError(ex,
                    $"{_MIGRATION_FAILED_MESSAGE} - CatalogTenantID: {{sourceSecretCatalogTenantID}} CatalogID: {{sourceSecretCatalogID}}, SecretPath: {{destinationSecretPath}}",
                    sourceSecretCatalogTenantID,
                    sourceSecretCatalogID,
                    destinationSecretPath
                );
                var errorModel = new ErrorModel(
                    ex,
                    addToErrorTab: true,
                    message: _MIGRATION_FAILED_MESSAGE)
                {
                    WorkspaceId = workspaceID
                };
                _errorService.Value.Log(errorModel);
            }

            return isMigrated;
        }
    }
}
