using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using Moq;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class IntegrationPointProfileManagerTests : KeplerSecurityTestsBase
    {
        private IIntegrationPointProfileManager _sut;
        private int _INTEGRATION_POINT_ARTIFACT_ID = 554556;

        private Mock<IIntegrationPointProfileRepository> _integrationPointProfileRepositoryFake;

        public override void SetUp()
        {
            base.SetUp();

            _integrationPointProfileRepositoryFake = new Mock<IIntegrationPointProfileRepository>();

            Container.Register(Component.For<IIntegrationPointProfileRepository>()
                .UsingFactoryMethod(_ => _integrationPointProfileRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            _sut = new IntegrationPointProfileManager(_loggerFake.Object, _permissionRepositoryFactoryFake.Object,
                Container);
        }

        [IdentifiedTestCase("7DEB25D8-55CC-4AB9-B07B-DA160365DABC", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("E7128653-CC75-4A7F-B357-6AD999699625", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("8FF3E176-46C7-4FA3-890E-70BDFC9B476C", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("D8DBCC4E-710E-4177-80F2-53EF082B3B52", true, true, 10, 20, "exampleName", "exampleEmail")]
        public void UserPermissionsToCreateIntegrationPointProfileVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            IntegrationPointModel expectedIntegrationPointModel = new IntegrationPointModel
            {
                ArtifactId = expectedArtifactId,
                DestinationProvider = expectedDestinationProvider,
                Name = expectedName,
                EmailNotificationRecipients = expectedEmailNotificationRecipients
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _integrationPointProfileRepositoryFake
                .Setup(x => x.CreateIntegrationPointProfile(createIntegrationPointRequest))
                .Returns(expectedIntegrationPointModel);

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.CreateIntegrationPointProfileAsync(createIntegrationPointRequest).Result,
                integrationPointModel, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("E06D7F78-55C5-4CA1-8DC8-6B92806F3A94", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("9C195000-2508-4982-A11A-3F76E052C488", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("D2586914-FAA8-48BB-8877-455EB9494D1E", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("797917CF-D119-4019-8FC5-3828723177DE", true, true, 10, 20, "exampleName", "exampleEmail")]
        public void UserPermissionsToUpdateIntegrationPointProfileVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            IntegrationPointModel expectedIntegrationPointModel = new IntegrationPointModel
            {
                ArtifactId = expectedArtifactId,
                DestinationProvider = expectedDestinationProvider,
                Name = expectedName,
                EmailNotificationRecipients = expectedEmailNotificationRecipients
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _integrationPointProfileRepositoryFake
                .Setup(x => x.UpdateIntegrationPointProfile(createIntegrationPointRequest))
                .Returns(expectedIntegrationPointModel);

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.UpdateIntegrationPointProfileAsync(createIntegrationPointRequest).Result,
                integrationPointModel, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("B5747736-3E78-4C95-8E3F-230A6C1035F3", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("E6125A65-973D-4C3A-BE2D-534503A3FAF2", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("92E38881-CB3D-42CA-A844-0E8F3E407EBD", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("040BF4FD-6623-4797-AAB2-5AE3063ABE7F", true, true, 10, 20, "exampleName", "exampleEmail")]
        public void UserPermissionsToGetIntegrationPointProfileVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            IntegrationPointModel expectedIntegrationPointModel = new IntegrationPointModel
            {
                ArtifactId = expectedArtifactId,
                DestinationProvider = expectedDestinationProvider,
                Name = expectedName,
                EmailNotificationRecipients = expectedEmailNotificationRecipients
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetIntegrationPointProfile(_INTEGRATION_POINT_ARTIFACT_ID))
                .Returns(expectedIntegrationPointModel);

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.GetIntegrationPointProfileAsync(_WORKSPACE_ID, _INTEGRATION_POINT_ARTIFACT_ID).Result,
                integrationPointModel, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("F7BD0D03-0F61-4E1B-A9C7-86B0671422AC", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("7D924D91-756C-4FB7-904A-27AE650E0A5F", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("9E8AE4A2-488D-4FF3-90A8-70C9B25BA204", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("B808A728-FCCA-470C-BDFE-9216601F2B2D", true, true, 10, 20, "exampleName", "exampleEmail")]
        public void UserPermissionsToGetAllIntegrationPointProfilesVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            IList<IntegrationPointModel> integrationPointModels = new List<IntegrationPointModel>
            {
                new IntegrationPointModel
                {
                    ArtifactId = 0,
                    DestinationProvider = 0,
                    Name = null,
                    EmailNotificationRecipients = null
                }
            };

            IList<IntegrationPointModel> expectedIntegrationPointModels = new List<IntegrationPointModel>
            {
                new IntegrationPointModel
                {
                    ArtifactId = expectedArtifactId,
                    DestinationProvider = expectedDestinationProvider,
                    Name = expectedName,
                    EmailNotificationRecipients = expectedEmailNotificationRecipients
                }
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetAllIntegrationPointProfiles())
                .Returns(expectedIntegrationPointModels);

            // Act
            integrationPointModels = ActAndGetResult(
                () => _sut.GetAllIntegrationPointProfilesAsync(_WORKSPACE_ID).Result,
                integrationPointModels, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            integrationPointModels[0].ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModels[0].DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModels[0].Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModels[0].EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("3F6B3362-265A-42F8-90B7-7DE57C645734", false, false, 0, null)]
        [IdentifiedTestCase("6629AD33-45FA-49A8-81E4-61388581B7C1", false, true, 0, null)]
        [IdentifiedTestCase("B972DE9D-9D3D-4052-919A-E4CEDFA0B1D4", true, false, 0, null)]
        [IdentifiedTestCase("3699065D-3FF0-42D8-A682-F2056B913011", true, true, 10, "exampleName")]
        public void UserPermissionsToGetOverwriteFieldsChoicesVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions, int expectedArtifactId, string expectedName)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            IList<OverwriteFieldsModel> overwriteFieldsModels = new List<OverwriteFieldsModel>
            {
                new OverwriteFieldsModel
                {
                    ArtifactId = 0,
                    Name = null
                }
            };

            IList<OverwriteFieldsModel> expectedOverwriteFieldsModels = new List<OverwriteFieldsModel>
            {
                new OverwriteFieldsModel
                {
                    ArtifactId = expectedArtifactId,
                    Name = expectedName
                }
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetOverwriteFieldChoices())
                .Returns(expectedOverwriteFieldsModels);

            // Act
            overwriteFieldsModels = ActAndGetResult(
                () => _sut.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Result,
                overwriteFieldsModels, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            overwriteFieldsModels[0].ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            overwriteFieldsModels[0].Name.ShouldBeEquivalentTo(expectedName);
        }

        [IdentifiedTestCase("E6FBD59C-FF90-4186-B4A0-28F72D8F7A52", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("630C1A3E-5D69-4D25-B0C1-015D346A6A49", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("C1B7B802-5CD6-484B-96E9-99BF4BA5787E", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("16082CA4-A5B6-4F10-BEA4-137D431C155C", true, true, 10, 20, "exampleName", "exampleEmail")]
        public void UserPermissionsToCreateIntegrationPointProfileFromIntegrationPointVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            string profileName = "example profile";
            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            IntegrationPointModel expectedIntegrationPointModel = new IntegrationPointModel
            {
                ArtifactId = expectedArtifactId,
                DestinationProvider = expectedDestinationProvider,
                Name = expectedName,
                EmailNotificationRecipients = expectedEmailNotificationRecipients
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _integrationPointProfileRepositoryFake
                .Setup(x => x.CreateIntegrationPointProfileFromIntegrationPoint(_INTEGRATION_POINT_ARTIFACT_ID, profileName))
                .Returns(expectedIntegrationPointModel);

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.CreateIntegrationPointProfileFromIntegrationPointAsync(_WORKSPACE_ID, _INTEGRATION_POINT_ARTIFACT_ID, profileName)
                    .Result, integrationPointModel, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

    }
}
