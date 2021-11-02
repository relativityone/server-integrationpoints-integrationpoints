using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Services;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class JobHistoryManagerTests : KeplerSecurityTestBase
    {
        private IJobHistoryManager _sut;

        [SetUp]
        public void Setup() => _sut = new JobHistoryManager(Logger, PermissionRepositoryFactory, Container);
        
        [Test]
        public void GetJobHistoryAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForJobHistoryManager>(() =>
            {
                JobHistoryRequest request = new JobHistoryRequest { WorkspaceArtifactId = WorkspaceId };
                return _sut.GetJobHistoryAsync(request);
            });
        }
        
        [TestCaseSource(typeof(PermissionsForJobHistoryManager))]
        public void GetJobHistoryAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups, () =>
            {
                JobHistoryRequest request = new JobHistoryRequest { WorkspaceArtifactId = WorkspaceId };
                return _sut.GetJobHistoryAsync(request);
            });
        }

        #region Permissions
     
        class PermissionsForJobHistoryManager : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId),
                new PermissionSetup
                {
                    Workspace = WorkspaceId,
                    Permission = new PermissionRef
                    {
                        Name = ObjectTypes.JobHistory,
                        PermissionType = PermissionType.View,
                        ArtifactType = new ArtifactTypeIdentifier(ObjectTypeGuids.JobHistoryGuid)
                    }
                }
            };
        }
        
        #endregion
    }
}
