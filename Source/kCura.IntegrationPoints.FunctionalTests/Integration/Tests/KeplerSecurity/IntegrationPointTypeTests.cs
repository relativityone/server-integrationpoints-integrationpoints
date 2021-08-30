using System.Collections.Generic;
using FluentAssertions;
using Relativity.IntegrationPoints.Services;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class IntegrationPointTypeTests : KeplerSecurityTestsBase
    {
        private IIntegrationPointTypeManager _sut;

        public override void SetUp()
        {
            base.SetUp();

            _sut = new IntegrationPointTypeManager(Logger, PermissionRepositoryFactory, Container);
        }

        [IdentifiedTestCase("D5995683-160D-459A-B36C-3D2D7F24AF4A", false, false, null, 0)]
        [IdentifiedTestCase("510F61F2-F0AC-4407-A708-6BDF7A282E93", false, true, null, 0)]
        [IdentifiedTestCase("6347274A-A1CF-4127-BB85-873FBB39173A", true, false, null, 0)]
        [IdentifiedTestCase("776530F7-FFC1-4CD3-83B7-618D288308AD", true, true, "Adler Sieben", 10)]
        public void UserPermissionsToGetJobHistoryVerification(bool workspaceAccessPermissionsValue, bool artifactTypePermissionsValue,
            string expectedName, int expectedArtifactId)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue, artifactTypePermissionsValue);
            IList<IntegrationPointTypeModel> integrationPointTypeModel = new List<IntegrationPointTypeModel>
            {
                new IntegrationPointTypeModel
                {
                    Name = null,
                    ArtifactId = 0
                }
            };

            // Act
            integrationPointTypeModel = ActAndGetResult(() => _sut.GetIntegrationPointTypes(SourceWorkspace.ArtifactId).Result,
                integrationPointTypeModel, workspaceAccessPermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            integrationPointTypeModel[0].Name.ShouldBeEquivalentTo(expectedName);
            integrationPointTypeModel[0].ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
        }

    }
}
