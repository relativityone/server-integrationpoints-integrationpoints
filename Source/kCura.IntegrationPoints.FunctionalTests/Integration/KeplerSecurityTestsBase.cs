using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using LanguageExt;
using Moq;
using Relativity.API;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Logging;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    class KeplerSecurityTestsBase : TestsBase
    {
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

            Logger = Container.Resolve<ILog>();
            PermissionRepositoryFactory = Container.Resolve<IPermissionRepositoryFactory>();

            PermissionRepository = PermissionRepositoryFactory.Create(Helper, SourceWorkspace.ArtifactId);
        }

        protected void Arrange(bool? workspaceAccessPermissions = null, bool? artifactTypePermissions = null)
        {
            if (workspaceAccessPermissions != null) SetUserWorkspaceAccessPermissions((bool)workspaceAccessPermissions);
            if (artifactTypePermissions != null) SetUserArtifactTypePermissions((bool)artifactTypePermissions);
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

        protected void Assert(RepositoryPermissions expectedRepositoryPermissions)
        {
            PermissionRepository.UserHasPermissionToAccessWorkspace()
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasWorkspaceAccessPermissions);
            PermissionRepository.UserCanEditDocuments()
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserCanEditDocuments);
            PermissionRepository.UserCanExport()
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserCanExport);
            PermissionRepository.UserCanImport()
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserCanImport);
            PermissionRepository.UserHasArtifactTypePermission(ArtifactTypeId, ArtifactPermission.Create)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasCreatePermissions);
            PermissionRepository.UserHasArtifactTypePermission(ArtifactTypeId, ArtifactPermission.Edit)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasEditPermissions);
            PermissionRepository.UserHasArtifactTypePermission(ArtifactTypeId, ArtifactPermission.View)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasViewPermissions);
            PermissionRepository.UserHasArtifactTypePermission(ArtifactTypeId, ArtifactPermission.Delete)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasDeletePermissions);
            PermissionRepository.UserHasArtifactInstancePermission(ArtifactTypeId, ArtifactId, ArtifactPermission.Create)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasCreateInstancePermissions);
            PermissionRepository.UserHasArtifactInstancePermission(ArtifactTypeId, ArtifactId, ArtifactPermission.Edit)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasEditInstancePermissions);
            PermissionRepository.UserHasArtifactInstancePermission(ArtifactTypeId, ArtifactId, ArtifactPermission.View)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasViewInstancePermissions);
            PermissionRepository.UserHasArtifactInstancePermission(ArtifactTypeId, ArtifactId, ArtifactPermission.Delete)
                .ShouldBeEquivalentTo(expectedRepositoryPermissions.UserHasDeleteInstancePermissions);
            }

        protected void SetUserWorkspaceAccessPermissions(bool permissionValue)
        {
            List<PermissionRef> permissions = new List<PermissionRef> {new PermissionRef()};
            List<PermissionValue> permissionValues = new List<PermissionValue>
            {
                new PermissionValue
                {
                    PermissionID = ArtifactId,
                    Selected = permissionValue
                }
            };

            Proxy.PermissionManager.Mock.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
                permissions, ArtifactId)).Returns(Task.FromResult(permissionValues));
        }

        protected void SetUserArtifactTypePermissions(bool permissionValue)
        {
            List<PermissionRef> permissions = new List<PermissionRef> {new PermissionRef()};
            List<PermissionValue> permissionValues = new List<PermissionValue>
            {
                new PermissionValue
                {
                    PermissionID = ArtifactId,
                    Selected = permissionValue
                }
            };

            Proxy.PermissionManager.Mock.Setup(x => x.GetPermissionSelectedAsync(SourceWorkspace.ArtifactId,
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
