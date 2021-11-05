using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity;
using Relativity.Services;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class PermissionManagerStub : KeplerStubBase<IPermissionManager>
    {
        private Dictionary<string, bool> _setPermissions = new Dictionary<string, bool>();

        public PermissionManagerStub()
        {
            Mock.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>()))
                .Returns((int workspaceId, List<PermissionRef> permissions) =>
                {
                    return Task.FromResult(permissions.Where(p =>
                    {
                        string permissionKey = GetPermissionKey(p, workspaceId);
                        if (_setPermissions.ContainsKey(permissionKey))
                        {
                            return _setPermissions[permissionKey];
                        }

                        return GrantNotConfiguredPermissions;
                    }).Select(p => new PermissionValue
                    {
                        ArtifactType = p.ArtifactType,
                        Name = p.Name,
                        PermissionType = p.PermissionType,
                        PermissionID = p.PermissionID,
                        Selected = true
                    }).ToList());
                });
            
            Mock.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>()))
                .Returns((int workspaceId, List<PermissionRef> permissions, int instanceId) =>
                {
                    return Task.FromResult(permissions.Where(p =>
                    {
                        string permissionKey = GetPermissionKey(p, workspaceId, instanceId);
                        if (_setPermissions.ContainsKey(permissionKey))
                        {
                            return _setPermissions[permissionKey];
                        }

                        return GrantNotConfiguredPermissions;
                    }).Select(p => new PermissionValue
                    {
                        ArtifactType = p.ArtifactType,
                        Name = p.Name,
                        PermissionType = p.PermissionType,
                        PermissionID = p.PermissionID,
                        Selected = true
                    }).ToList());
                });
        }

        public bool GrantNotConfiguredPermissions { get; set; } = true;

        public void SetupPermission(PermissionModel permissionModel, int workspaceId, int? instanceId = null, bool granted = true)
        {
            _setPermissions[GetPermissionKey(permissionModel, workspaceId, instanceId)] = granted;
        }
        
        public void SetupPermission(PermissionRef permissionModel, int workspaceId, int? instanceId = null, bool granted = true)
        {
            _setPermissions[GetPermissionKey(permissionModel, workspaceId, instanceId)] = granted;
        }

        public void SetupPermission(PermissionSetup permissionSetup) => SetupPermission(permissionSetup.Permission,
            permissionSetup.Workspace, permissionSetup.Instance, permissionSetup.Granted);

        public void SetupPermissionToWorkspace(int workspaceId, bool granted)
        {
            _setPermissions[GetWorkspaceKey(workspaceId)] = granted;
        }
        
        private static PermissionType ArtifactPermissionToPermissionType(ArtifactPermission artifactPermission)
        {
            switch (artifactPermission)
            {
                case ArtifactPermission.View:
                    return PermissionType.View;
                case ArtifactPermission.Edit:
                    return PermissionType.Edit;
                case ArtifactPermission.Create:
                    return PermissionType.Add;
                case ArtifactPermission.Delete:
                    return PermissionType.Delete;
                default:
                    throw new System.Exception("Invalid ArtifactPermission");
            }
        }
        
        private static string GetWorkspaceKey(int workspaceId) => GetPermissionKey(new PermissionRef()
        {
            ArtifactType = new ArtifactTypeIdentifier((int) ArtifactType.Case),
            PermissionType = PermissionType.View
        }, -1, workspaceId);

        private static string GetPermissionKey(PermissionModel permissionModel, int workspaceId, int? instanceId = null) =>
            $"{permissionModel.ObjectTypeGuid};{ArtifactPermissionToPermissionType(permissionModel.ArtifactPermission).Name};{workspaceId};{instanceId}";

        private static string GetPermissionTypeDescription(ArtifactTypeIdentifier permissionRefArtifactType)
        {
            if (permissionRefArtifactType.Guids.Any())
            {
                return permissionRefArtifactType.Guids.First().ToString();
            }

            return permissionRefArtifactType.ID.ToString();
        }
        
        private static string GetPermissionKey(PermissionRef permissionRef, int workspaceId, int? instanceId = null) =>
            $"{GetPermissionTypeDescription(permissionRef.ArtifactType)};{permissionRef.PermissionType.Name};{workspaceId};{instanceId}";
    }
}