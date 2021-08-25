using System.Collections.Generic;
using FluentAssertions;
using Relativity.IntegrationPoints.Services;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class IntegrationPointManagerTests : KeplerSecurityTestsBase
    {
        private IIntegrationPointManager _sut;
        private readonly int _INTEGRATION_POINT_ARTIFACT_ID = ArtifactProvider.NextId();

        public override void SetUp()
        {
            base.SetUp();

            _sut = new IntegrationPointManager(Logger, PermissionRepositoryFactory, Container);
        }

        [IdentifiedTestCase("EB9CFEBC-C65C-418C-AD64-28E2E8076A40", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("E0572DA7-4C55-486F-984E-8CD05995B8D7", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("CD265661-463E-4E49-B74C-9065E0C1D894", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("CD403829-62C5-4F3D-961C-324D74B14C25", true, true, 10, 11, "Adler Sieben", "Adler Sieben")]
        public void UserPermissionsToCreateIntegrationPointVerification(bool workspaceOrArtifactInstancePermissionsValue,
            bool artifactTypePermissionsValue, int expectedArtifactId, int expectedDestinationProvider, 
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.CreateIntegrationPointAsync(createIntegrationPointRequest).Result,
                integrationPointModel, workspaceOrArtifactInstancePermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("92ABF2AC-ACD8-4BD7-8065-7044BC31BF7C", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("7C11A0F1-F847-47A4-B20F-D4AA414C28B8", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("5D7560B7-FBAE-4171-95E9-72D69B27D43B", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("487EDC16-CC35-48ED-85CC-D487551C8743", true, true, 10, 11, "Adler Sieben", "Adler Sieben")]
        public void UserPermissionsToUpdateIntegrationPointVerification(bool workspaceOrArtifactInstancePermissionsValue,
            bool artifactTypePermissionsValue, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            UpdateIntegrationPointRequest updateIntegrationPointRequest = new UpdateIntegrationPointRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.UpdateIntegrationPointAsync(updateIntegrationPointRequest).Result,
                integrationPointModel, workspaceOrArtifactInstancePermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("983F5AF3-5B31-4B89-9541-D6AB2F78BF31", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("920FC297-2CE2-46C8-BCF0-94A1E1CBCE50", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("2407AB79-FE2F-4BF5-B199-630C400C575D", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("17F449C5-2840-4CB7-B427-E9F553E599CB", true, true, 10, 11, "Adler Sieben", "Adler Sieben")]
        public void UserPermissionsToGetIntegrationPointVerification(bool workspaceOrArtifactInstancePermissionsValue,
            bool artifactTypePermissionsValue, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.GetIntegrationPointAsync(SourceWorkspace.ArtifactId, _INTEGRATION_POINT_ARTIFACT_ID).Result,
                integrationPointModel, workspaceOrArtifactInstancePermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("20DC8562-4CA3-44A2-AD1A-D9EE8D2ED73D", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("9CFCA838-853D-46EF-B526-098F9894DC2F", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("9DC0AED8-0DE2-4519-867B-1CB75111525E", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("9408E02C-104C-467C-BB17-48D39039D4D5", true, true, 10, 11, "Adler Sieben", "Adler Sieben")]
        public void UserPermissionsToRunIntegrationPointVerification(bool workspaceOrArtifactInstancePermissionsValue,
            bool artifactTypePermissionsValue, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
            object integrationPointModelObject = new IntegrationPointModel();

            // Act
            integrationPointModelObject = ActAndGetResult(
                () => _sut.RunIntegrationPointAsync(SourceWorkspace.ArtifactId, _INTEGRATION_POINT_ARTIFACT_ID).Result,
                integrationPointModelObject, workspaceOrArtifactInstancePermissionsValue & artifactTypePermissionsValue);

            IntegrationPointModel integrationPointModel = (IntegrationPointModel) integrationPointModelObject;

            // Assert
            Assert();
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("1E438253-EC91-4D4D-AB61-6ED7682A0869", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("4007D4F8-0F65-4B5A-AA26-9DE8C2C6B5D8", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("EA6376CB-51D5-4A44-AACC-F778CF745571", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("2C00B19E-BC43-4594-A36A-D3C0BE26CDF3", true, true, 10, 11, "Adler Sieben", "Adler Sieben")]
        public void UserPermissionsToGetAllIntegrationPointsVerification(bool workspaceOrArtifactInstancePermissionsValue,
            bool artifactTypePermissionsValue, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
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
            
            // Act
            integrationPointModels = ActAndGetResult(
                () => _sut.GetAllIntegrationPointsAsync(SourceWorkspace.ArtifactId).Result,
                integrationPointModels, workspaceOrArtifactInstancePermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            integrationPointModels[0].ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModels[0].DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModels[0].Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModels[0].EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("0D065A1B-4F8A-4962-8AA1-F64A30706335", false, false, "", 0)]
        [IdentifiedTestCase("4EC62AE9-A849-4DA7-91D4-364B00B4514E", false, true, "", 0)]
        [IdentifiedTestCase("71120E2F-6C57-4EDC-BD7F-C3F25735DA04", true, false, "", 0)]
        [IdentifiedTestCase("C6F4D8B2-A685-447A-9438-AC7D797D6F79", true, true, "Adler Sieben", 10)]
        public void UserPermissionsToGetOverwriteFieldsChoicesVerification(bool workspaceOrArtifactInstancePermissionsValue,
            bool artifactTypePermissionsValue, string expectedName, int expectedArtifactId)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
            IList<OverwriteFieldsModel> overwriteFieldsModels = new List<OverwriteFieldsModel>
            {
                new OverwriteFieldsModel
                {
                    Name = "",
                    ArtifactId = 0
                }
            };
            
            // Act
            overwriteFieldsModels = ActAndGetResult(
                () => _sut.GetOverwriteFieldsChoicesAsync(SourceWorkspace.ArtifactId).Result,
                overwriteFieldsModels, workspaceOrArtifactInstancePermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            overwriteFieldsModels[0].Name.ShouldBeEquivalentTo(expectedName);
            overwriteFieldsModels[0].ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
        }

        [IdentifiedTestCase("F4E419C9-FADE-46AC-8813-D6CD749D74B8", false, false, 0, 0, null, null)]
        [IdentifiedTestCase("298AAAB7-EE8B-480B-BA16-FD06157266B7", false, true, 0, 0, null, null)]
        [IdentifiedTestCase("D7C78E60-F4A7-4CBD-956E-C6EBEB4D1054", true, false, 0, 0, null, null)]
        [IdentifiedTestCase("66DE7478-7B3E-4431-9A5A-A8551A120DBD", true, true, 10, 11, "Adler Sieben", "Adler Sieben")]
        public void UserPermissionsToCreateIntegrationPointFromProfileVerification(bool workspaceOrArtifactInstancePermissionsValue,
            bool artifactTypePermissionsValue, int expectedArtifactId, int expectedDestinationProvider,
            string expectedName, string expectedEmailNotificationRecipients)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
            IntegrationPointModel integrationPointModel = new IntegrationPointModel();
            int profileArtifactId = 32142;
            string integrationPointName = "integration point example name";

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.CreateIntegrationPointFromProfileAsync(SourceWorkspace.ArtifactId, profileArtifactId, integrationPointName).Result,
                integrationPointModel, workspaceOrArtifactInstancePermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            integrationPointModel.ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(expectedDestinationProvider);
            integrationPointModel.Name.ShouldBeEquivalentTo(expectedName);
            integrationPointModel.EmailNotificationRecipients.ShouldBeEquivalentTo(expectedEmailNotificationRecipients);
        }

        [IdentifiedTestCase("968D972F-5562-4A08-9B62-CD2CDEA8AC96", false, false, -1)]
        [IdentifiedTestCase("94051369-EB33-4A54-AD80-2C99765A80AA", false, true, -1)]
        [IdentifiedTestCase("3E7481B0-8E71-4042-88FE-1990B3FB9722", true, false, -1)]
        [IdentifiedTestCase("7E733FDF-6502-4B76-A317-E1C07DD164F2", true, true, 10)]
        public void UserPermissionsToGetIntegrationPointArtifactTypeIdVerification(bool workspaceOrArtifactInstancePermissionsValue,
            bool artifactTypePermissionsValue, int expectedArtifactId)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue, artifactTypePermissionsValue);
            int artifactTypeId = -1;

            // Act
            artifactTypeId = ActAndGetResult(
                () => _sut.GetIntegrationPointArtifactTypeIdAsync(SourceWorkspace.ArtifactId).Result,
                artifactTypeId, workspaceOrArtifactInstancePermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            artifactTypeId.ShouldBeEquivalentTo(expectedArtifactId);
        }

    }
}
