using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Services;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    public class IntegrationPointManagerTests : KeplerSecurityTestBase
    {
        private IntegrationPointManager _sut;
        private readonly int IntegrationPointPofile = 420; 


        [SetUp]
        public void Setup()
        {
            _sut = new IntegrationPointManager(Logger, Container.Resolve<IPermissionRepositoryFactory>(), Container);
        }

        [Test]
        public void CreateIntegrationPointAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForCreateIntegraionPoint>(() =>
            {
                CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };
            
                // Act
                return _sut.CreateIntegrationPointAsync(createIntegrationPointRequest);
            });
        }

        [TestCaseSource(typeof(PermissionsForCreateIntegraionPoint))]
        public void CreateIntegrationPointAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
            {
                CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };
            
                // Act
                return _sut.CreateIntegrationPointAsync(createIntegrationPointRequest);
            });
        }
        
        [Test]
        public void UpdateIntegrationPointAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForUpdateIntegraionPoint>(() =>
            {
                UpdateIntegrationPointRequest updateRequest = new UpdateIntegrationPointRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };
            
                // Act
                return _sut.UpdateIntegrationPointAsync(updateRequest);
            });
        }

        [TestCaseSource(typeof(PermissionsForUpdateIntegraionPoint))]
        public void UpdateIntegrationPointAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
            {
                UpdateIntegrationPointRequest updateRequest = new UpdateIntegrationPointRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };
            
                // Act
                return _sut.UpdateIntegrationPointAsync(updateRequest);
            });
        }
        
        [Test]
        public void CreateIntegrationPointFromProfileAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForCreateIntegrationPointFromProfileAsync>(() =>
            {
                // Act
                return _sut.CreateIntegrationPointFromProfileAsync(WorkspaceId, IntegrationPointPofile, "Adler Sieben");
            });
        }

        [TestCaseSource(typeof(PermissionsForCreateIntegrationPointFromProfileAsync))]
        public void CreateIntegrationPointFromProfileAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
            {
                return _sut.CreateIntegrationPointFromProfileAsync(WorkspaceId, IntegrationPointPofile, "Adler Sieben");
            });
        }

        #region Permissions
        
        class PermissionsForCreateIntegraionPoint : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPoint,
                        PermissionType = PermissionType.Add,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointGuid)
                    }
                }
            };
        }
        
        class PermissionsForUpdateIntegraionPoint : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPoint,
                        PermissionType = PermissionType.Edit,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointGuid)
                    }
                }
            };
        }

        class PermissionsForCreateIntegrationPointFromProfileAsync : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPoint,
                        PermissionType = PermissionType.Add,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointGuid)
                    }
                },
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPointProfile,
                        PermissionType = PermissionType.View,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointProfileGuid)
                    }
                }
            };
        }

        #endregion
    }
}