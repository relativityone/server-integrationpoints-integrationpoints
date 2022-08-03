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
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointManagerTests : TestBase
    {
        private const int _WORKSPACE_ID = 266818;
        private IntegrationPointManager _integrationPointManager;
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

            _integrationPointManager = new IntegrationPointManager(_logger, permissionRepositoryFactory, _container);
        }

        [Test]
        public void ItShouldGrantAccessForView()
        {
            // Arrange 
            const int requiredNumberOfCalls = 4;

            // Act
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(true);

            _integrationPointManager.GetAllIntegrationPointsAsync(_WORKSPACE_ID).Wait();
            _integrationPointManager.GetIntegrationPointArtifactTypeIdAsync(_WORKSPACE_ID).Wait();
            _integrationPointManager.GetIntegrationPointAsync(_WORKSPACE_ID, 870372).Wait();
            _integrationPointManager.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Wait();

            // Assert
            _permissionRepository.Received(requiredNumberOfCalls).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(requiredNumberOfCalls).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View);
        }

        [Test]
        public void ItShouldGrantAccessForCreate()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create).Returns(true);

            _integrationPointManager.CreateIntegrationPointAsync(new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            }).Wait();

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);
        }

        [Test]
        public void ItShouldGrantAccessForCreateFromProfile()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View).Returns(true);

            _integrationPointManager.CreateIntegrationPointFromProfileAsync(_WORKSPACE_ID, 965598, "ip_755").Wait();

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View);
        }

        [Test]
        public void ItShouldGrantAccessForEdit()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Edit).Returns(true);

            _integrationPointManager.RunIntegrationPointAsync(_WORKSPACE_ID, 531917).Wait();
            _integrationPointManager.UpdateIntegrationPointAsync(new UpdateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            }).Wait();

            _permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Edit);
        }

        [Test]
        [TestCase(false, false, "Workspace, Integration Point - View")]
        [TestCase(false, true, "Workspace")]
        [TestCase(true, false, "Integration Point - View")]
        public void ItShouldDenyAccessForViewAndLogIt(bool workspaceAccess, bool viewAccess, string missingPermissions)
        {
            // Arrange 
            const int requiredNumberOfCalls = 4;
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(viewAccess);

            // Act
            Assert.That(() => _integrationPointManager.GetAllIntegrationPointsAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _integrationPointManager.GetIntegrationPointArtifactTypeIdAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _integrationPointManager.GetIntegrationPointAsync(_WORKSPACE_ID, 790529).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _integrationPointManager.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            // Assert
            _permissionRepository.Received(requiredNumberOfCalls).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(requiredNumberOfCalls).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetAllIntegrationPointsAsync",
                    missingPermissions);
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetIntegrationPointArtifactTypeIdAsync",
                    missingPermissions);
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetIntegrationPointAsync",
                    missingPermissions);
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetOverwriteFieldsChoicesAsync",
                    missingPermissions);
        }

        [Test]
        [TestCase(false, false, "Workspace, Integration Point - Create")]
        [TestCase(false, true, "Workspace")]
        [TestCase(true, false, "Integration Point - Create")]
        public void ItShouldDenyAccessForCreateAndLogIt(bool workspaceAccess, bool createAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create).Returns(createAccess);

            var request = new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            Assert.That(() => _integrationPointManager.CreateIntegrationPointAsync(request).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "CreateIntegrationPointAsync",
                    missingPermissions);
        }

        [Test]
        [TestCase(false, false, "Workspace, Integration Point - Edit")]
        [TestCase(false, true, "Workspace")]
        [TestCase(true, false, "Integration Point - Edit")]
        public void ItShouldDenyAccessForEditAndLogIt(bool workspaceAccess, bool editAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Edit).Returns(editAccess);

            var request = new UpdateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            Assert.That(() => _integrationPointManager.UpdateIntegrationPointAsync(request).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _integrationPointManager.RunIntegrationPointAsync(_WORKSPACE_ID, 122990).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Edit);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "UpdateIntegrationPointAsync",
                    missingPermissions);
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "RunIntegrationPointAsync",
                    missingPermissions);
        }

        [Test]
        [TestCase(false, false, false, "Workspace, Integration Point - Create, Integration Point Profile - View")]
        [TestCase(false, true, false, "Workspace, Integration Point Profile - View")]
        [TestCase(false, false, true, "Workspace, Integration Point - Create")]
        [TestCase(false, true, true, "Workspace")]
        [TestCase(true, false, false, "Integration Point - Create, Integration Point Profile - View")]
        [TestCase(true, false, true, "Integration Point - Create")]
        [TestCase(true, true, false, "Integration Point Profile - View")]
        public void ItShouldDenyAccessForCreateFromProfileAndLogIt(bool workspaceAccess, bool createIPAccess, bool viewProfileAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create).Returns(createIPAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View).Returns(viewProfileAccess);

            Assert.That(() => _integrationPointManager.CreateIntegrationPointFromProfileAsync(_WORKSPACE_ID, 464409, "ip_430").Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "CreateIntegrationPointFromProfileAsync",
                    missingPermissions);
        }

        [Test]
        public void ItShouldHideAndLogException()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Edit).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View).Returns(true);

            var expectedException = new ArgumentException();

            var integrationPointRepository = Substitute.For<Services.Repositories.IIntegrationPointRepository>();
            integrationPointRepository.CreateIntegrationPoint(Arg.Any<CreateIntegrationPointRequest>()).Throws(expectedException);
            integrationPointRepository.UpdateIntegrationPoint(Arg.Any<UpdateIntegrationPointRequest>()).Throws(expectedException);
            integrationPointRepository.GetIntegrationPoint(Arg.Any<int>()).Throws(expectedException);
            integrationPointRepository.RunIntegrationPoint(Arg.Any<int>(), Arg.Any<int>()).Throws(expectedException);
            integrationPointRepository.GetAllIntegrationPoints().Throws(expectedException);
            integrationPointRepository.GetOverwriteFieldChoices().Throws(expectedException);
            integrationPointRepository.CreateIntegrationPointFromProfile(Arg.Any<int>(), Arg.Any<string>()).Throws(expectedException);
            integrationPointRepository.GetIntegrationPointArtifactTypeId().Throws(expectedException);

            _container.Resolve<Services.Repositories.IIntegrationPointRepository>().Returns(integrationPointRepository);

            Assert.That(() => _integrationPointManager.CreateIntegrationPointAsync(new CreateIntegrationPointRequest
                {
                    WorkspaceArtifactId = _WORKSPACE_ID
                }).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointManager.UpdateIntegrationPointAsync(new UpdateIntegrationPointRequest
                {
                    WorkspaceArtifactId = _WORKSPACE_ID
                }).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointManager.GetIntegrationPointAsync(_WORKSPACE_ID, 647698).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointManager.RunIntegrationPointAsync(_WORKSPACE_ID, 356677).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointManager.GetAllIntegrationPointsAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointManager.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointManager.CreateIntegrationPointFromProfileAsync(_WORKSPACE_ID, 176513, "ip_811").Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _integrationPointManager.GetIntegrationPointArtifactTypeIdAsync(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "CreateIntegrationPointAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "UpdateIntegrationPointAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetIntegrationPointAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "RunIntegrationPointAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetAllIntegrationPointsAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetOverwriteFieldsChoicesAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "CreateIntegrationPointFromProfileAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetIntegrationPointArtifactTypeIdAsync");
        }
    }
}