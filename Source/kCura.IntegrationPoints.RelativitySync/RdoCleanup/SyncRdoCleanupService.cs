using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync.RdoCleanup
{
    public class SyncRdoCleanupService : ISyncRdoCleanupService
    {
        private readonly Guid _progressObjectType = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");
        private readonly Guid _batchObjectType = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
        private readonly Guid _syncConfigurationObjectType = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;

        public SyncRdoCleanupService(IServicesMgr servicesMgr, IAPILog logger)
        {
            _servicesMgr = servicesMgr;
            _logger = logger;
        }

        public async Task DeleteSyncRdosAsync(int workspaceId)
        {
            await CascadeDeleteObjectTypeAsync(workspaceId, _progressObjectType).ConfigureAwait(false);
            await CascadeDeleteObjectTypeAsync(workspaceId, _batchObjectType).ConfigureAwait(false);
            await CascadeDeleteObjectTypeAsync(workspaceId, _syncConfigurationObjectType).ConfigureAwait(false);
        }

        private async Task CascadeDeleteObjectTypeAsync(int workspaceId, Guid objectTypeGuid)
        {
            using (IArtifactGuidManager artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                bool objectTypeExists = await artifactGuidManager.GuidExistsAsync(workspaceId, objectTypeGuid).ConfigureAwait(false);

                if (!objectTypeExists)
                {
                    _logger.LogInformation("Object Type GUID: '{guid}' doesn't exist in workspace {workspaceId}", objectTypeGuid, workspaceId);
                    return;
                }
            }

            await MassDeleteObjectsAsync(workspaceId, objectTypeGuid).ConfigureAwait(false);
            await DeleteObjectTypeAsync(workspaceId, objectTypeGuid).ConfigureAwait(false);
        }

        private async Task MassDeleteObjectsAsync(int workspaceId, Guid objectTypeGuid)
        {
            try
            {
                using (IObjectManager objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
                {
                    MassDeleteResult massDeleteResult = await objectManager.DeleteAsync(workspaceId, new MassDeleteByCriteriaRequest()
                    {
                        ObjectIdentificationCriteria = new ObjectIdentificationCriteria()
                        {
                            ObjectType = new ObjectTypeRef()
                            {
                                Guid = objectTypeGuid
                            }
                        }
                    }).ConfigureAwait(false);
                    _logger.LogInformation("Mass delete objects of type '{guid}' success: {success} message: {message}", objectTypeGuid, massDeleteResult.Success, massDeleteResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mass delete objects of type: '{guid}'", objectTypeGuid);
            }
        }

        private async Task DeleteObjectTypeAsync(int workspaceId, Guid objectTypeGuid)
        {
            using (IArtifactGuidManager artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            using (IObjectTypeManager objectTypeManager = _servicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
            {
                try
                {
                    int objectTypeId = await artifactGuidManager.ReadSingleArtifactIdAsync(workspaceId, objectTypeGuid).ConfigureAwait(false);

                    await objectTypeManager.DeleteAsync(workspaceId, objectTypeId).ConfigureAwait(false);

                    _logger.LogInformation("Deleted Object Type Artifact ID: {artifactId} GUID: '{guid}'", objectTypeId, objectTypeGuid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete Object Type GUID: '{guid}'", objectTypeGuid);
                }
            }
        }
    }
}