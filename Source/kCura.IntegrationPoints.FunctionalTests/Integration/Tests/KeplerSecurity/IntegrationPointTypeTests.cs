using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Services;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class IntegrationPointTypeTests : KeplerSecurityTestBase
    {
        private IIntegrationPointTypeManager _sut;

        [SetUp]
        public void Setup() => _sut = new IntegrationPointTypeManager(Logger, PermissionRepositoryFactory, Container);

        [Test]
        public void GetOverwriteFieldsChoicesAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForGetIntegrationPointTypes>(() => 
                _sut.GetIntegrationPointTypes(WorkspaceId));
        }
        
        [TestCaseSource(typeof(PermissionsForGetIntegrationPointTypes))]
        public void GetOverwriteFieldsChoicesAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() => 
                _sut.GetIntegrationPointTypes(WorkspaceId));
        }

        #region Permissions

        class PermissionsForGetIntegrationPointTypes : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.IntegrationPointType,
                        PermissionType = PermissionType.View,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.IntegrationPointTypeGuid)
                    }
                }
            };
        }

        #endregion
    }
}
