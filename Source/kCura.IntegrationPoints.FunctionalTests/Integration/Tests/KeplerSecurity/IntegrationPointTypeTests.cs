using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Moq;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class IntegrationPointTypeTests : KeplerSecurityTestsBase
    {
        private IIntegrationPointTypeManager _sut;

        public override void SetUp()
        {
            base.SetUp();

            _sut = new IntegrationPointTypeManager(_loggerFake.Object, _permissionRepositoryFactoryFake.Object, Container);
        }

        [IdentifiedTestCase("D5995683-160D-459A-B36C-3D2D7F24AF4A", false, false)]
        [IdentifiedTestCase("510F61F2-F0AC-4407-A708-6BDF7A282E93", false, true)]
        [IdentifiedTestCase("6347274A-A1CF-4127-BB85-873FBB39173A", true, false)]
        [IdentifiedTestCase("776530F7-FFC1-4CD3-83B7-618D288308AD", true, true)]
        public void UserPermissionsToGetJobHistoryVerification(bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
         
            IList<IntegrationPointTypeModel> integrationPointTypeModel = new List<IntegrationPointTypeModel>();

            Mock<IIntegrationPointTypeRepository> integrationPointTypeRepositoryFake = new Mock<IIntegrationPointTypeRepository>();
            integrationPointTypeRepositoryFake.Setup(x => x.GetIntegrationPointTypes()).Returns(new List<IntegrationPointTypeModel>());

            Container.Register(Component.For<IIntegrationPointTypeRepository>()
                .UsingFactoryMethod(_ => integrationPointTypeRepositoryFake.Object).LifestyleTransient().IsDefault());

            // Act
            integrationPointTypeModel = ActAndGetResult(() => _sut.GetIntegrationPointTypes(_WORKSPACE_ID).Result,
                integrationPointTypeModel, workspaceAccessPermissions & artifactTypePermissions);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(expectedRepositoryPermissions);
        }

    }
}
