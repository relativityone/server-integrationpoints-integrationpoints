using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Moq;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class IntegrationPointProfileManagerTests : KeplerSecurityTestsBase
    {
        private IIntegrationPointProfileManager _sut;
        private int _INTEGRATION_POINT_ARTIFACT_ID = 554556;

        public override void SetUp()
        {
            base.SetUp();

            _sut = new IntegrationPointProfileManager(_loggerFake.Object, _permissionRepositoryFactoryFake.Object,
                Container);
        }

        [IdentifiedTestCase("4EE1C0B2-81A7-4C2B-9568-014C9CE100D1", false, false)]
        [IdentifiedTestCase("48976A72-2F7C-48DC-BE01-4E13B8B051B7", false, true)]
        [IdentifiedTestCase("713E4D2D-00F2-454F-A09C-DB1B77E061DD", true, false)]
        [IdentifiedTestCase("1FE4C284-0E6F-4FEF-A579-0601D4679819", true, true)]
        public void UserPermissionsToCreateIntegrationPointProfileVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            Mock<IIntegrationPointProfileRepository> integrationPointProfileRepositoryFake =
                new Mock<IIntegrationPointProfileRepository>();
            integrationPointProfileRepositoryFake
                .Setup(x => x.CreateIntegrationPointProfile(new CreateIntegrationPointRequest()))
                .Returns(new IntegrationPointModel());

            Container.Register(Component.For<IIntegrationPointProfileRepository>()
                .UsingFactoryMethod(_ => integrationPointProfileRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.CreateIntegrationPointProfileAsync(createIntegrationPointRequest).Result,
                integrationPointModel);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(-1, -1, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("D68972EA-2481-417A-B47D-E5A600CA7977", false, false)]
        [IdentifiedTestCase("B7646BEB-0623-40B3-B74D-7C988AA0E334", false, true)]
        [IdentifiedTestCase("D03BDACD-3351-40E1-A307-EE43D7F35175", true, false)]
        [IdentifiedTestCase("E5D1DF46-1811-4B38-B895-FED61A2BB98E", true, true)]
        public void UserPermissionsToUpdateIntegrationPointProfileVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            Mock<IIntegrationPointProfileRepository> integrationPointProfileRepositoryFake =
                new Mock<IIntegrationPointProfileRepository>();
            integrationPointProfileRepositoryFake
                .Setup(x => x.UpdateIntegrationPointProfile(new CreateIntegrationPointRequest()))
                .Returns(new IntegrationPointModel());

            Container.Register(Component.For<IIntegrationPointProfileRepository>()
                .UsingFactoryMethod(_ => integrationPointProfileRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.UpdateIntegrationPointProfileAsync(createIntegrationPointRequest).Result,
                integrationPointModel);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(-1, -1, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("282D8773-B99D-4FE5-9831-E36E28E2C91E", false, false)]
        [IdentifiedTestCase("9E7364A4-7FB8-4FA0-A3AA-6D56EDBFBAE8", false, true)]
        [IdentifiedTestCase("43F7DC21-E4B7-4258-AE90-57F63E1D2D74", true, false)]
        [IdentifiedTestCase("0B4CDFD4-9691-4E27-AC94-7857D09CF242", true, true)]
        public void UserPermissionsToGetIntegrationPointProfileVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            Mock<IIntegrationPointProfileRepository> integrationPointProfileRepositoryFake =
                new Mock<IIntegrationPointProfileRepository>();
            integrationPointProfileRepositoryFake
                .Setup(x => x.GetIntegrationPointProfile(_WORKSPACE_ID))
                .Returns(new IntegrationPointModel());

            Container.Register(Component.For<IIntegrationPointProfileRepository>()
                .UsingFactoryMethod(_ => integrationPointProfileRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.GetIntegrationPointProfileAsync(_WORKSPACE_ID, _INTEGRATION_POINT_ARTIFACT_ID).Result,
                integrationPointModel);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(-1, -1, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("A4E543B1-AE52-428F-A0C0-B2A6A088ED09", false, false)]
        [IdentifiedTestCase("5962A606-F2A5-4596-AAC9-DEE1BB091324", false, true)]
        [IdentifiedTestCase("280B87EB-F8D2-4070-91F7-77BC1412BC7B", true, false)]
        [IdentifiedTestCase("09A02C9F-B602-4497-B88D-2DD50B712A5F", true, true)]
        public void UserPermissionsToGetAllIntegrationPointProfilesVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            IList<IntegrationPointModel> integrationPointModels = new List<IntegrationPointModel>();

            Mock<IIntegrationPointProfileRepository> integrationPointProfileRepositoryFake =
                new Mock<IIntegrationPointProfileRepository>();
            integrationPointProfileRepositoryFake
                .Setup(x => x.GetAllIntegrationPointProfiles())
                .Returns(new List<IntegrationPointModel>());

            Container.Register(Component.For<IIntegrationPointProfileRepository>()
                .UsingFactoryMethod(_ => integrationPointProfileRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            // Act
            integrationPointModels = ActAndGetResult(
                () => _sut.GetAllIntegrationPointProfilesAsync(_WORKSPACE_ID).Result,
                integrationPointModels);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(-1, -1, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("3F6B3362-265A-42F8-90B7-7DE57C645734", false, false)]
        [IdentifiedTestCase("6629AD33-45FA-49A8-81E4-61388581B7C1", false, true)]
        [IdentifiedTestCase("B972DE9D-9D3D-4052-919A-E4CEDFA0B1D4", true, false)]
        [IdentifiedTestCase("3699065D-3FF0-42D8-A682-F2056B913011", true, true)]
        public void UserPermissionsToGetOverwriteFieldsChoicesVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            IList<OverwriteFieldsModel> overwriteFieldsModels = new List<OverwriteFieldsModel>();

            Mock<IIntegrationPointProfileRepository> integrationPointProfileRepositoryFake =
                new Mock<IIntegrationPointProfileRepository>();
            integrationPointProfileRepositoryFake
                .Setup(x => x.GetOverwriteFieldChoices())
                .Returns(new List<OverwriteFieldsModel>());

            Container.Register(Component.For<IIntegrationPointProfileRepository>()
                .UsingFactoryMethod(_ => integrationPointProfileRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            // Act
            overwriteFieldsModels = ActAndGetResult(
                () => _sut.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Result,
                overwriteFieldsModels);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(-1, -1, expectedRepositoryPermissions);
        }

    }
}
