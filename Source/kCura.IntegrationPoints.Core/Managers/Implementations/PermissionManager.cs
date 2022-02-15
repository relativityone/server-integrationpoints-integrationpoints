using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class PermissionManager : IPermissionManager
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public PermissionManager(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public bool UserCanImport(int workspaceArtifactId)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserCanImport();
        }

        public bool UserCanExport(int workspaceArtifactId)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserCanExport();
        }

        public bool UserCanEditDocuments(int workspaceArtifactId)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserCanEditDocuments();
        }

        public bool UserHasArtifactTypePermission(int workspaceArtifactId, int artifactTypeId, ArtifactPermission artifactPermission)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserHasArtifactTypePermission(artifactTypeId, artifactPermission);
        }

        public bool UserHasArtifactTypePermission(int workspaceArtifactId, Guid artifactTypeGuid, ArtifactPermission artifactPermission)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserHasArtifactTypePermission(artifactTypeGuid, artifactPermission);
        }

        public bool UserHasArtifactInstancePermission(int workspaceArtifactId, Guid artifactTypeGuid, int artifactId, ArtifactPermission artifactPermission)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserHasArtifactInstancePermission(artifactTypeGuid, artifactId, artifactPermission);
        }

        public bool UserHasArtifactInstancePermission(int workspaceArtifactId, int artifactTypeId, int artifactId, ArtifactPermission artifactPermission)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserHasArtifactInstancePermission(artifactTypeId, artifactId, artifactPermission);
        }

        public bool UserHasArtifactTypePermissions(int workspaceArtifactId, int artifactTypeId, IEnumerable<ArtifactPermission> artifactPermissions)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserHasArtifactTypePermissions(artifactTypeId, artifactPermissions);
        }

        public bool UserHasPermissionToAccessWorkspace(int workspaceArtifactId)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserHasPermissionToAccessWorkspace();
        }
    }
}
