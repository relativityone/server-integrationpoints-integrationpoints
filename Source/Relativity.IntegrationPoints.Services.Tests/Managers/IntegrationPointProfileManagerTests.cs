using System;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointProfileManagerTests : TestBase
    {
        private const int _WORKSPACE_ID = 819434;
        private IntegrationPointProfileManager _integrationPointProfileManager;
        private IPermissionRepository _permissionRepository;
        private ILog _logger;
        private IWindsorContainer _container;

        public override void SetUp()
        {
            _logger = Substitute.For<ILog>();
            _permissionRepository = Substitute.For<IPermissionRepository>();
            _container = Substitute.For<IWindsorContainer>();

            var permissionRepositoryFactory = Substitute.For<IPermissionRepositoryFactory>();
            permissionRepositoryFactory.Create(Arg.Any<IHelper>(), _WORKSPACE_ID).Returns(_permissionRepository);

            _integrationPointProfileManager = new IntegrationPointProfileManager(_logger, permissionRepositoryFactory, _container);
        }

        [Test]
        public void ItShouldGrantAccessForView()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View).Returns(true);

            _integrationPointProfileManager.GetAllIntegrationPointProfilesAsync(_WORKSPACE_ID).Wait();
            _integrationPointProfileManager.GetIntegrationPointProfileAsync(_WORKSPACE_ID, 802556).Wait();
            _integrationPointProfileManager.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Wait();

            _permissionRepository.Received(3).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(3).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View);
        }

        [Test]
        public void ItShouldGrantAccessForCreate()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create).Returns(true);

            _integrationPointProfileManager.CreateIntegrationPointProfileAsync(new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            }).Wait();

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create);
        }

        [Test]
        public void ItShouldGrantAccessForCreateFromIntegrationPoint()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(true);

            _integrationPointProfileManager.CreateIntegrationPointProfileFromIntegrationPointAsync(_WORKSPACE_ID, 985242, "ip_401").Wait();

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create);
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View);
        }

        [Test]
        public void ItShouldGrantAccessForEdit()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Edit).Returns(true);

            _integrationPointProfileManager.UpdateIntegrationPointProfileAsync(new UpdateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            }).Wait();

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Edit);
        }

        [Test]
        [TestCase(false, false, "Workspace, Integration Point Profile - View")]
        [TestCase(false, true, "Workspace")]
        [TestCase(true, false, "Integration Point Profile - View")]
        public void ItShouldDenyAccessForViewAndLogIt(bool workspaceAccess, bool viewAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View).Returns(viewAccess);

            Assert.That(() => _integrationPointProfileManager.GetAllIntegrationPointProfilesAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _integrationPointProfileManager.GetIntegrationPointProfileAsync(_WORKSPACE_ID, 486418).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _integrationPointProfileManager.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(3).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(3).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetAllIntegrationPointProfilesAsync",
                    missingPermissions);
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetIntegrationPointProfileAsync",
                    missingPermissions);
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetOverwriteFieldsChoicesAsync",
                    missingPermissions);
        }

        [Test]
        [TestCase(false, false, "Workspace, Integration Point Profile - Create")]
        [TestCase(false, true, "Workspace")]
        [TestCase(true, false, "Integration Point Profile - Create")]
        public void ItShouldDenyAccessForCreateAndLogIt(bool workspaceAccess, bool createAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create).Returns(createAccess);

            var request = new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            Assert.That(() => _integrationPointProfileManager.CreateIntegrationPointProfileAsync(request).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "CreateIntegrationPointProfileAsync",
                    missingPermissions);
        }

        [Test]
        [TestCase(false, false, "Workspace, Integration Point Profile - Edit")]
        [TestCase(false, true, "Workspace")]
        [TestCase(true, false, "Integration Point Profile - Edit")]
        public void ItShouldDenyAccessForEditAndLogIt(bool workspaceAccess, bool editAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Edit).Returns(editAccess);

            var request = new UpdateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            Assert.That(() => _integrationPointProfileManager.UpdateIntegrationPointProfileAsync(request).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Edit);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "UpdateIntegrationPointProfileAsync",
                    missingPermissions);
        }

        [Test]
        [TestCase(false, false, false, "Workspace, Integration Point Profile - Create, Integration Point - View")]
        [TestCase(false, true, false, "Workspace, Integration Point - View")]
        [TestCase(false, false, true, "Workspace, Integration Point Profile - Create")]
        [TestCase(false, true, true, "Workspace")]
        [TestCase(true, false, false, "Integration Point Profile - Create, Integration Point - View")]
        [TestCase(true, false, true, "Integration Point Profile - Create")]
        [TestCase(true, true, false, "Integration Point - View")]
        public void ItShouldDenyAccessForCreateFromIntegrationPointAndLogIt(bool workspaceAccess, bool createIPAccess, bool viewProfileAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create).Returns(createIPAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(viewProfileAccess);

            Assert.That(() => _integrationPointProfileManager.CreateIntegrationPointProfileFromIntegrationPointAsync(_WORKSPACE_ID, 170861, "ip_902").Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create);
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.",
                    "CreateIntegrationPointProfileFromIntegrationPointAsync",
                    missingPermissions);
        }

        [Test]
        public void ItShouldHideAndLogException()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Edit).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(true);

            var expectedException = new ArgumentException();

            var integrationPointProfileRepository = Substitute.For<IIntegrationPointProfileAccessor>();
            integrationPointProfileRepository.CreateIntegrationPointProfile(Arg.Any<CreateIntegrationPointRequest>()).Throws(expectedException);
            integrationPointProfileRepository.UpdateIntegrationPointProfile(Arg.Any<UpdateIntegrationPointRequest>()).Throws(expectedException);
            integrationPointProfileRepository.GetIntegrationPointProfile(Arg.Any<int>()).Throws(expectedException);
            integrationPointProfileRepository.GetAllIntegrationPointProfiles().Throws(expectedException);
            integrationPointProfileRepository.GetOverwriteFieldChoices().Throws(expectedException);
            integrationPointProfileRepository.CreateIntegrationPointProfileFromIntegrationPoint(Arg.Any<int>(), Arg.Any<string>()).Throws(expectedException);

            _container.Resolve<IIntegrationPointProfileAccessor>().Returns(integrationPointProfileRepository);

            Assert.That(() => _integrationPointProfileManager.CreateIntegrationPointProfileAsync(new CreateIntegrationPointRequest
                {
                    WorkspaceArtifactId = _WORKSPACE_ID
                }).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointProfileManager.UpdateIntegrationPointProfileAsync(new UpdateIntegrationPointRequest
                {
                    WorkspaceArtifactId = _WORKSPACE_ID
                }).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointProfileManager.GetIntegrationPointProfileAsync(_WORKSPACE_ID, 789402).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointProfileManager.GetAllIntegrationPointProfilesAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointProfileManager.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointProfileManager.CreateIntegrationPointProfileFromIntegrationPointAsync(_WORKSPACE_ID, 788159, "ip_877").Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "CreateIntegrationPointProfileAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "UpdateIntegrationPointProfileAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetIntegrationPointProfileAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetAllIntegrationPointProfilesAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetOverwriteFieldsChoicesAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "CreateIntegrationPointProfileFromIntegrationPointAsync");
        }
    }
}