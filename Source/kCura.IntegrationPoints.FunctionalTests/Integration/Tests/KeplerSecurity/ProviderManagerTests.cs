using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Services;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class ProviderManagerTests : KeplerSecurityTestBase
    {
        private IProviderManager _sut;

        [SetUp]
        public void Setup() => _sut = new ProviderManager(Logger, PermissionRepositoryFactory, Container);

        [Test]
        public void GetDestinationProviders_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForGettingDestinationProviders>(() =>
                _sut.GetDestinationProviders(WorkspaceId));
        }

        [TestCaseSource(typeof(PermissionsForGettingDestinationProviders))]
        public void GetDestinationProviders_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups, () =>
                _sut.GetDestinationProviders(WorkspaceId));
        }

        [Test]
        public void GetSourceProviders_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForGettingSourceProviders>(() =>
                _sut.GetSourceProviders(WorkspaceId));
        }

        [TestCaseSource(typeof(PermissionsForGettingSourceProviders))]
        public void GetSourceProviders_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups, () =>
                _sut.GetSourceProviders(WorkspaceId));
        }

        [Test]
        public void GetSourceProviderArtifactIdAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForGettingSourceProviders>(() =>
                _sut.GetSourceProviderArtifactIdAsync(WorkspaceId, string.Empty));
        }

        [TestCaseSource(typeof(PermissionsForGettingSourceProviders))]
        public void GetSourceProviderArtifactIdAsync_ShouldThrowInsufficientPermissions(
            PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups, () =>
                _sut.GetSourceProviderArtifactIdAsync(WorkspaceId, string.Empty));
        }

        [Test]
        public void GetDestinationProviderArtifactIdAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForGettingDestinationProviders>(() =>
                _sut.GetDestinationProviderArtifactIdAsync(WorkspaceId, string.Empty));
        }

        [TestCaseSource(typeof(PermissionsForGettingDestinationProviders))]
        public void GetDestinationProviderArtifactIdAsync_ShouldThrowInsufficientPermissions(
            PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups, () =>
                _sut.GetDestinationProviderArtifactIdAsync(WorkspaceId, string.Empty));
        }

        [Test]
        public void InstallProviderAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForInstallingSourceProviders>(() =>
            {
                InstallProviderRequest request = new InstallProviderRequest
                {
                    WorkspaceID = WorkspaceId
                };

                return _sut.InstallProviderAsync(request);
            });
        }

        [TestCaseSource(typeof(PermissionsForInstallingSourceProviders))]
        public void InstallProviderAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups, () =>
            {
                InstallProviderRequest request = new InstallProviderRequest
                {
                    WorkspaceID = WorkspaceId
                };

                return _sut.InstallProviderAsync(request);
            });
        }

        [Test]
        public void UninstallProviderAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForUninstallingSourceProviders>(() =>
            {
                UninstallProviderRequest request = new UninstallProviderRequest
                {
                    WorkspaceID = WorkspaceId
                };

                return _sut.UninstallProviderAsync(request);
            });
        }

        [TestCaseSource(typeof(PermissionsForUninstallingSourceProviders))]
        public void UninstallProviderAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups, () =>
            {
                UninstallProviderRequest request = new UninstallProviderRequest
                {
                    WorkspaceID = WorkspaceId
                };

                return _sut.UninstallProviderAsync(request);
            });
        }

        #region Permissions

        class PermissionsForGettingDestinationProviders : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.DestinationProvider,
                        PermissionType = PermissionType.View,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.DestinationProviderGuid)
                    }
                }
            };
        }

        class PermissionsForGettingSourceProviders : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.SourceProvider,
                        PermissionType = PermissionType.View,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.SourceProviderGuid)
                    }
                }
            };
        }

        class PermissionsForInstallingSourceProviders : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.SourceProvider,
                        PermissionType = PermissionType.Add,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.SourceProviderGuid)
                    }
                },
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.SourceProvider,
                        PermissionType = PermissionType.Edit,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.SourceProviderGuid)
                    }
                }
            };
        }

        class PermissionsForUninstallingSourceProviders : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.SourceProvider,
                        PermissionType = PermissionType.Delete,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.SourceProviderGuid)
                    }
                },
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPoint,
                        PermissionType = PermissionType.Edit,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointGuid)
                    }
                },
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPoint,
                        PermissionType = PermissionType.Delete,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointGuid)
                    }
                }
            };
        }

        #endregion
    }
}
