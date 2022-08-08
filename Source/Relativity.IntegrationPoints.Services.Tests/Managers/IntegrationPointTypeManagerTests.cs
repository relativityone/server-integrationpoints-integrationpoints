using System;
using System.Collections.Generic;
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
    public class IntegrationPointTypeManagerTests : TestBase
    {
        private const int _WORKSPACE_ID = 784838;
        private IntegrationPointTypeManager _integrationPointTypeManager;
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

            _integrationPointTypeManager = new IntegrationPointTypeManager(_logger, permissionRepositoryFactory, _container);
        }

        [Test]
        public void ItShouldGrantAccess()
        {
            MockValidPermissions();

            _integrationPointTypeManager.GetIntegrationPointTypes(_WORKSPACE_ID).Wait();

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointType), ArtifactPermission.View);
        }

        [Test]
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void ItShouldDenyAccess(bool workspaceAccess, bool integrationPointTypeAccess)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointType), ArtifactPermission.View).Returns(integrationPointTypeAccess);


            Assert.That(() => _integrationPointTypeManager.GetIntegrationPointTypes(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointType), ArtifactPermission.View);
        }

        [Test]
        [TestCase(false, false, "Workspace, Integration Point Type - View")]
        [TestCase(true, false, "Integration Point Type - View")]
        [TestCase(false, true, "Workspace")]
        public void ItShouldLogDenyingAccess(bool workspaceAccess, bool integrationPointTypeAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointType), ArtifactPermission.View).Returns(integrationPointTypeAccess);

            try
            {
                _integrationPointTypeManager.GetIntegrationPointTypes(_WORKSPACE_ID).Wait();
            }
            catch (Exception)
            {
                //Ignore as this test checks logging only
            }

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetIntegrationPointTypes", missingPermissions);
        }

        [Test]
        public void ItShouldGetIntegrationPointTypes()
        {
            MockValidPermissions();

            var integrationPointTypeRepository = Substitute.For<IIntegrationPointTypeRepository>();
            _container.Resolve<IIntegrationPointTypeRepository>().Returns(integrationPointTypeRepository);

            var expectedResult = new List<IntegrationPointTypeModel>();

            integrationPointTypeRepository.GetIntegrationPointTypes().Returns(expectedResult);

            var actualResult = _integrationPointTypeManager.GetIntegrationPointTypes(_WORKSPACE_ID).Result;

            integrationPointTypeRepository.Received(1).GetIntegrationPointTypes();

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldHideException()
        {
            MockValidPermissions();

            var integrationPointTypeRepository = Substitute.For<IIntegrationPointTypeRepository>();
            integrationPointTypeRepository.GetIntegrationPointTypes().Throws(new ArgumentException());
            _container.Resolve<IIntegrationPointTypeRepository>().Returns(integrationPointTypeRepository);

            Assert.That(() => _integrationPointTypeManager.GetIntegrationPointTypes(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
        }

        [Test]
        public void ItShouldLogException()
        {
            MockValidPermissions();

            var expectedException = new ArgumentException();

            var integrationPointTypeRepository = Substitute.For<IIntegrationPointTypeRepository>();
            integrationPointTypeRepository.GetIntegrationPointTypes().Throws(expectedException);
            _container.Resolve<IIntegrationPointTypeRepository>().Returns(integrationPointTypeRepository);

            try
            {
                _integrationPointTypeManager.GetIntegrationPointTypes(_WORKSPACE_ID).Wait();
            }
            catch (Exception)
            {
                //Ignore as this test checks logging only
            }

            _logger.Received(1).LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetIntegrationPointTypes");
        }

        private void MockValidPermissions()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointType), ArtifactPermission.View).Returns(true);
        }
    }
}