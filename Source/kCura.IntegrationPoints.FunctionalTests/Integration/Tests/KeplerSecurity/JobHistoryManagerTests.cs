using FluentAssertions;
using Relativity.IntegrationPoints.Services;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class JobHistoryManagerTests : KeplerSecurityTestsBase
    {
        private IJobHistoryManager _sut;

        public override void SetUp()
        {
            base.SetUp();
            
            _sut = new JobHistoryManager(Logger, PermissionRepositoryFactory, Container);
        }

        [IdentifiedTestCase("A41778E7-2D25-49FA-9919-ECD90B2168BD", false, false, 0, 0, 0)]
        [IdentifiedTestCase("DFAED5F1-2DC8-4CAA-8BDD-C99542DE537D", false, true, 0, 0, 0)]
        [IdentifiedTestCase("55190915-CEE6-4AB9-99A0-5B2586E5A84B", true, false, 0, 0, 0)]
        [IdentifiedTestCase("8E0588E1-60E0-4876-A220-04C8557565D7", true, true, 1, 10, 20)]
        public void UserPermissionsToGetJobHistoryVerification( bool workspaceAccessPermissionsValue, bool artifactTypePermissionsValue,
            int expectedDataSize, long expectedTotalAvailable, long expectedTotalDocumentsPushed)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue, artifactTypePermissionsValue);

            JobHistoryRequest jobHistoryRequest = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            JobHistorySummaryModel jobHistorySummaryModel = new JobHistorySummaryModel();
            
            // Act
            jobHistorySummaryModel = ActAndGetResult(() => _sut.GetJobHistoryAsync(jobHistoryRequest).Result,
                jobHistorySummaryModel, workspaceAccessPermissionsValue & artifactTypePermissionsValue);

            // Assert
            Assert();
            jobHistorySummaryModel.Data.Length.ShouldBeEquivalentTo(expectedDataSize);
            jobHistorySummaryModel.TotalAvailable.ShouldBeEquivalentTo(expectedTotalAvailable);
            jobHistorySummaryModel.TotalDocumentsPushed.ShouldBeEquivalentTo(expectedTotalDocumentsPushed);
        }

    }
}
