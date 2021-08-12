using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using Relativity.API;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    class KeplerSecurityTestsBase : TestsBase
    {
        protected const int _WORKSPACE_ID = 266818;
        protected const int _SAVEDSEARCH_ID = 4324;
        protected const int _VIEW_ID = 1234;

        protected IPermissionRepository _permissionRepository;

        protected Mock<IPermissionManager> _permissionManagerFake;
        protected Mock<IPermissionRepositoryFactory> _permissionRepositoryFactoryFake;

        public override void SetUp()
        {
            base.SetUp();
            _permissionRepository = new PermissionRepository(Helper, _WORKSPACE_ID);

            _permissionRepositoryFactoryFake = new Mock<IPermissionRepositoryFactory>();

            _permissionManagerFake = new Mock<IPermissionManager>();
            _permissionRepositoryFactoryFake.Setup(x => x.Create(null, _WORKSPACE_ID))
                .Returns(_permissionRepository);

            Mock<IServicesMgr> serviceManagerFake = Helper.GetServicesManagerMock();
            serviceManagerFake.Setup(x => x.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
                .Returns(_permissionManagerFake.Object);

        }

        protected void Arrange(bool? workspaceAccessPermissions = null, bool? artifactTypePermissions = null)
        {
            if (workspaceAccessPermissions != null) SetUserWorkspaceAccessPermissions((bool)workspaceAccessPermissions);
            if (artifactTypePermissions != null) SetUserArtifactTypePermissions((bool)artifactTypePermissions);
        }

        protected int ActAndGetResult(Func<int> function)
        {
            int total = -1;

            try
            {
                total = function();
            }
            catch (AggregateException exceptions)
            {
                exceptions.InnerExceptions[0].Should().BeOfType<InsufficientPermissionException>();
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            return total;
        }

        protected void Assert(int total, int expectedTotal, RepositoryPermissions expectedRepositoryPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace()
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasWorkspaceAccessPermissions);
            _permissionRepository.UserCanEditDocuments()
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserCanEditDocuments);
            _permissionRepository.UserCanExport()
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserCanExport);
            _permissionRepository.UserCanImport()
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserCanImport);
            _permissionRepository.UserHasArtifactTypePermission(_WORKSPACE_ID, ArtifactPermission.Create)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasCreatePermissions);
            _permissionRepository.UserHasArtifactTypePermission(_WORKSPACE_ID, ArtifactPermission.Edit)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasEditPermissions);
            _permissionRepository.UserHasArtifactTypePermission(_WORKSPACE_ID, ArtifactPermission.View)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasViewPermissions);
            _permissionRepository.UserHasArtifactTypePermission(_WORKSPACE_ID, ArtifactPermission.Delete)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasDeletePermissions);
            _permissionRepository.UserHasArtifactInstancePermission(_WORKSPACE_ID, _WORKSPACE_ID, ArtifactPermission.Create)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasCreateInstancePermissions);
            _permissionRepository.UserHasArtifactInstancePermission(_WORKSPACE_ID, _WORKSPACE_ID, ArtifactPermission.Edit)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasEditInstancePermissions);
            _permissionRepository.UserHasArtifactInstancePermission(_WORKSPACE_ID, _WORKSPACE_ID, ArtifactPermission.View)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasViewInstancePermissions);
            _permissionRepository.UserHasArtifactInstancePermission(_WORKSPACE_ID, _WORKSPACE_ID, ArtifactPermission.Delete)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasDeleteInstancePermissions);
            total.ShouldBeEquivalentTo(expectedTotal);
        }

        protected void SetUserWorkspaceAccessPermissions(bool permissionValue)
        {
            List<PermissionRef> permissions = new List<PermissionRef> {new PermissionRef()};
            List<PermissionValue> permissionValues = new List<PermissionValue>
            {
                new PermissionValue
                {
                    Selected = permissionValue
                }
            };

            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(-1,
                permissions, _WORKSPACE_ID)).Returns(Task.FromResult(permissionValues));
        }

        protected void SetUserArtifactTypePermissions(bool permissionValue)
        {
            List<PermissionRef> permissions = new List<PermissionRef> {new PermissionRef()};
            List<PermissionValue> permissionValues = new List<PermissionValue>
            {
                new PermissionValue
                {
                    Selected = permissionValue
                }
            };

            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(_WORKSPACE_ID,
                permissions)).Returns(Task.FromResult(permissionValues));
        }

        internal class RepositoryPermissions
        {
          
            internal bool UserHasWorkspaceAccessPermissions { get; set; }
            internal bool UserCanEditDocuments { get; set; }
            internal bool UserCanExport { get; set; }
            internal bool UserCanImport { get; set; }
            internal bool UserHasCreatePermissions { get; set; }
            internal bool UserHasEditPermissions { get; set; }
            internal bool UserHasViewPermissions { get; set; }
            internal bool UserHasDeletePermissions { get; set; }
            internal bool UserHasCreateInstancePermissions { get; set; }
            internal bool UserHasEditInstancePermissions { get; set; }
            internal bool UserHasViewInstancePermissions { get; set; }
            internal bool UserHasDeleteInstancePermissions { get; set; }

        }
    }
}
