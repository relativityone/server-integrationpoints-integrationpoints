using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using FluentAssertions;
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

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetAllIntegrationPointProfiles())
                .Returns(new List<IntegrationPointModel>());

            // Act
            integrationPointModels = ActAndGetResult(
                () => _sut.GetAllIntegrationPointProfilesAsync(_WORKSPACE_ID).Result,
                integrationPointModels, workspaceAccessPermissions & artifactTypePermissions);

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

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetOverwriteFieldChoices())
                .Returns(new List<OverwriteFieldsModel>());

            // Act
            overwriteFieldsModels = ActAndGetResult(
                () => _sut.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Result,
                overwriteFieldsModels, workspaceAccessPermissions & artifactTypePermissions);

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

        [IdentifiedTestCase("2B7E0B59-6718-4E18-9E56-BFE980E5371B", false, false)]
        [IdentifiedTestCase("7D980BFE-97A4-4F55-950A-AE3C9C0AF2C5", false, true)]
        [IdentifiedTestCase("D152F7AF-4D4D-4167-B6DD-D9FE49D5EC48", true, false)]
        [IdentifiedTestCase("F2CA13FB-17CB-485A-BC60-E9287F3C742A", true, true)]
        public void UserPermissionsToCreateIntegrationPointProfileFromIntegrationPointVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            string profileName = "example profile";

            IntegrationPointModel overwriteFieldsModel = new IntegrationPointModel();

            _integrationPointProfileRepositoryFake
                .Setup(x => x.CreateIntegrationPointProfileFromIntegrationPoint(_INTEGRATION_POINT_ARTIFACT_ID, profileName))
                .Returns(new IntegrationPointModel());

            // Act
            overwriteFieldsModel = ActAndGetResult(
                () => _sut.CreateIntegrationPointProfileFromIntegrationPointAsync(_WORKSPACE_ID, _INTEGRATION_POINT_ARTIFACT_ID, profileName)
                    .Result, overwriteFieldsModel, workspaceAccessPermissions & artifactTypePermissions);

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
