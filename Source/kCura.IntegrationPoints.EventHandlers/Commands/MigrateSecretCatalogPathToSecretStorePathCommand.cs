using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class MigrateSecretCatalogPathToSecretStorePathCommand : IEHCommand
    {
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly ISecretStoreMigrationService _migrationService;
        private readonly IWorkspaceContext _workspaceContext;
        private readonly IAPILog _apiLog;

        public MigrateSecretCatalogPathToSecretStorePathCommand(
            IRelativityObjectManager relativityObjectManager,
            ISecretStoreMigrationService migrationService,
            IWorkspaceContext workspaceContext,
            IAPILog apiLog)
        {
            _relativityObjectManager = relativityObjectManager;
            _migrationService = migrationService;
            _workspaceContext = workspaceContext;
            _apiLog = apiLog.ForContext<MigrateSecretCatalogPathToSecretStorePathCommand>();
        }

        public void Execute()
        {
            int workspaceID = _workspaceContext.GetWorkspaceID();
            IList<IntegrationPoint> integrationPoints = GetAllIntegrationPointsWithSetSecuredConfiguration();

            if (!integrationPoints.Any())
            {
                _apiLog.LogInformation("There was no integration point in a given workspace ({workspaceID}) that needs to be migrated to Secret Store.", workspaceID);
                return;
            }

            foreach (var integrationPoint in integrationPoints)
            {
                MigrateIntegrationPointSecrets(
                    integrationPoint,
                    workspaceID
                );
            }
        }

        private void MigrateIntegrationPointSecrets(
            IntegrationPoint integrationPoint,
            int workspaceID)
        {
            string secretID = integrationPoint.SecuredConfiguration;
            int integrationPointID = integrationPoint.ArtifactId;
            _migrationService.TryMigrateSecret(workspaceID, integrationPointID, secretID);
        }

        private IList<IntegrationPoint> GetAllIntegrationPointsWithSetSecuredConfiguration()
        {
            var query = new QueryRequest
            {
                Fields = new[]
                {
                    new FieldRef
                    {
                        Guid = IntegrationPointFieldGuids.SecuredConfigurationGuid
                    }
                },
                Condition = $"'{IntegrationPointFields.SecuredConfiguration}' ISSET"
            };

            return _relativityObjectManager.Query<IntegrationPoint>(query);
        }
    }
}
