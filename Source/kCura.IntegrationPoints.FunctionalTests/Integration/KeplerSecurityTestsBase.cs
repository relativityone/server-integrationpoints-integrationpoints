using System;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    class KeplerSecurityTestsBase : TestsBase
    {
        private RepositoryPermissions _expectedPermissionRepository;

        protected int ArtifactTypeId;
        protected int ArtifactId;

        protected IPermissionRepository PermissionRepository;
        protected IPermissionRepositoryFactory PermissionRepositoryFactory;
        protected ILog Logger;

        public override void SetUp()
        {
            base.SetUp();

            ArtifactTypeId = ArtifactProvider.NextId();
            ArtifactId = ArtifactProvider.NextId();
            _expectedPermissionRepository = new RepositoryPermissions();

            Logger = Container.Resolve<ILog>();
            PermissionRepositoryFactory = Container.Resolve<IPermissionRepositoryFactory>();

            PermissionRepository = PermissionRepositoryFactory.Create(Helper, SourceWorkspace.ArtifactId);
        }

        protected void Arrange(bool workspaceOrArtifactInstancePermissionsValue = false, bool artifactTypePermissionsValue = false)
        {
            SetupPermissionsCheck(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
            SetExpectedRepositoryPermissions(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
        }

        protected T ActAndGetResult<T>(Func<T> function, T initialResultValue, bool exceptionNotExpected)
        {
            T result = initialResultValue;

            if (exceptionNotExpected)
            {
                result = function();
            }
            else
            {
                bool exceptionFound = false;
                try
                {
                    result = function();
                }
                catch (AggregateException exceptions)
                {
                    exceptionFound = true;
                    exceptions.InnerExceptions[0].Should().BeOfType<InsufficientPermissionException>();
                }
                catch (Exception ex)
                {
                    exceptionFound = true;
                    ex.Should().BeOfType<InsufficientPermissionException>();
                }
                finally
                {
                    exceptionFound.Should().BeTrue();
                }
            }

            return result;
        }

        protected void Assert()
        {
            PermissionRepository.UserHasPermissionToAccessWorkspace()
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasWorkspaceAccessPermissions);
            PermissionRepository.UserCanEditDocuments()
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserCanEditDocuments);
            PermissionRepository.UserCanExport()
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserCanExport);
            PermissionRepository.UserCanImport()
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserCanImport);
            PermissionRepository.UserHasArtifactTypePermission(ArtifactTypeId, ArtifactPermission.Create)
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasCreatePermissions);
            PermissionRepository.UserHasArtifactTypePermission(ArtifactTypeId, ArtifactPermission.Edit)
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasEditPermissions);
            PermissionRepository.UserHasArtifactTypePermission(ArtifactTypeId, ArtifactPermission.View)
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasViewPermissions);
            PermissionRepository.UserHasArtifactTypePermission(ArtifactTypeId, ArtifactPermission.Delete)
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasDeletePermissions);
            PermissionRepository.UserHasArtifactInstancePermission(ArtifactTypeId, ArtifactId, ArtifactPermission.Create)
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasCreateInstancePermissions);
            PermissionRepository.UserHasArtifactInstancePermission(ArtifactTypeId, ArtifactId, ArtifactPermission.Edit)
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasEditInstancePermissions);
            PermissionRepository.UserHasArtifactInstancePermission(ArtifactTypeId, ArtifactId, ArtifactPermission.View)
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasViewInstancePermissions);
            PermissionRepository.UserHasArtifactInstancePermission(ArtifactTypeId, ArtifactId, ArtifactPermission.Delete)
                .ShouldBeEquivalentTo(_expectedPermissionRepository.UserHasDeleteInstancePermissions);
        }

        protected void SetupPermissionsCheck(bool workspaceOrArtifactInstancePermissionsValue, bool artifactTypePermissionValue)
        {
            Proxy.PermissionManager.SetupPermissionsCheck(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionValue);
        }

        private void SetExpectedRepositoryPermissions(bool workspaceOrArtifactInstancePermissionsValue, bool artifactTypePermissionValue)
        {
            _expectedPermissionRepository.UserHasWorkspaceAccessPermissions = workspaceOrArtifactInstancePermissionsValue;
            _expectedPermissionRepository.UserHasCreateInstancePermissions = workspaceOrArtifactInstancePermissionsValue;
            _expectedPermissionRepository.UserHasViewInstancePermissions = workspaceOrArtifactInstancePermissionsValue;
            _expectedPermissionRepository.UserHasEditInstancePermissions = workspaceOrArtifactInstancePermissionsValue;
            _expectedPermissionRepository.UserHasDeleteInstancePermissions = workspaceOrArtifactInstancePermissionsValue;
            _expectedPermissionRepository.UserHasCreatePermissions = artifactTypePermissionValue;
            _expectedPermissionRepository.UserHasEditPermissions = artifactTypePermissionValue;
            _expectedPermissionRepository.UserHasViewPermissions = artifactTypePermissionValue;
            _expectedPermissionRepository.UserHasDeletePermissions = artifactTypePermissionValue;
            _expectedPermissionRepository.UserCanEditDocuments = artifactTypePermissionValue;
            _expectedPermissionRepository.UserCanExport = artifactTypePermissionValue;
            _expectedPermissionRepository.UserCanImport = artifactTypePermissionValue;
        }

        private class RepositoryPermissions
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
