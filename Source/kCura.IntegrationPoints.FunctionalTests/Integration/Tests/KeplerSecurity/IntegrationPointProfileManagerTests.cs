using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Services;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class IntegrationPointProfileManagerTests : KeplerSecurityTestBase
    {
        private IIntegrationPointProfileManager _sut;
        private readonly int _INTEGRATION_POINT_ARTIFACT_ID = 420;

        [SetUp]
        public void Setup() => _sut = new IntegrationPointProfileManager(Logger, PermissionRepositoryFactory, Container);

        [Test]
        public void CreateIntegrationPointProfileAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForCreateIntegraionPointProfile>(() =>
            {
                CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.CreateIntegrationPointProfileAsync(createIntegrationPointRequest);
            });
        }

        [TestCaseSource(typeof(PermissionsForCreateIntegraionPointProfile))]
        public void CreateIntegrationPointProfileAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
            {
                CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.CreateIntegrationPointProfileAsync(createIntegrationPointRequest);
            });
        }

        [Test]
        public void CreateIntegrationPointProfileFromIntegrationPointAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForCreateIntegraionPointProfileFromIntegrationPoint>(() =>
                _sut.CreateIntegrationPointProfileFromIntegrationPointAsync(WorkspaceId, _INTEGRATION_POINT_ARTIFACT_ID, "Adler Sieben"));
        }

        [TestCaseSource(typeof(PermissionsForCreateIntegraionPointProfileFromIntegrationPoint))]
        public void CreateIntegrationPointProfileFromIntegrationPointAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
                _sut.CreateIntegrationPointProfileFromIntegrationPointAsync(WorkspaceId, _INTEGRATION_POINT_ARTIFACT_ID, "Adler Sieben"));
        }

        [Test]
        public void GetIntegrationPointProfileAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForGetIntegrationPointProfileAsync>(() =>
                _sut.GetIntegrationPointProfileAsync(WorkspaceId, _INTEGRATION_POINT_ARTIFACT_ID));
        }

        [TestCaseSource(typeof(PermissionsForGetIntegrationPointProfileAsync))]
        public void GetIntegrationPointProfileAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
                _sut.GetIntegrationPointProfileAsync(WorkspaceId, _INTEGRATION_POINT_ARTIFACT_ID));
        }

        [Test]
        public void GetAllIntegrationPointProfilesAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForGetIntegrationPointProfileAsync>(() =>
                _sut.GetAllIntegrationPointProfilesAsync(WorkspaceId));
        }

        [TestCaseSource(typeof(PermissionsForGetIntegrationPointProfileAsync))]
        public void GetAllIntegrationPointProfilesAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
                _sut.GetAllIntegrationPointProfilesAsync(WorkspaceId));
        }

        [Test]
        public void UpdateIntegrationPointProfileAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForUpdateIntegrationPointProfileAsync>(() =>
            {
                CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.UpdateIntegrationPointProfileAsync(createIntegrationPointRequest);
            });
        }

        [TestCaseSource(typeof(PermissionsForUpdateIntegrationPointProfileAsync))]
        public void UpdateIntegrationPointProfileAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
            {
                CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.UpdateIntegrationPointProfileAsync(createIntegrationPointRequest);
            });
        }

        [Test]
        public void GetOverwriteFieldsChoicesAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForGetIntegrationPointProfileAsync>(() =>
                _sut.GetOverwriteFieldsChoicesAsync(WorkspaceId));
        }

        [TestCaseSource(typeof(PermissionsForGetIntegrationPointProfileAsync))]
        public void GetOverwriteFieldsChoicesAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
                _sut.GetOverwriteFieldsChoicesAsync(WorkspaceId));
        }

        #region Permissions

        class PermissionsForCreateIntegraionPointProfile : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPointProfile,
                        PermissionType = PermissionType.Add,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointProfileGuid)
                    }
                }
            };
        }

        class PermissionsForCreateIntegraionPointProfileFromIntegrationPoint : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPointProfile,
                        PermissionType = PermissionType.Add,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointProfileGuid)
                    }
                },
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPoint,
                        PermissionType = PermissionType.View,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointGuid)
                    }
                }
            };
        }

        class PermissionsForGetIntegrationPointProfileAsync : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
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

        class PermissionsForUpdateIntegrationPointProfileAsync : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPointProfile,
                        PermissionType = PermissionType.Edit,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointProfileGuid)
                    }
                }
            };
        }

        #endregion
    }
}
