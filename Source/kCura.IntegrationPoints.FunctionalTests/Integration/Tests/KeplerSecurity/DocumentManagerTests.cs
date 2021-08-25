using System;
using FluentAssertions;
using Relativity.IntegrationPoints.Services;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class DocumentManagerTests : KeplerSecurityTestsBase
    {
        private IDocumentManager _sut;

        public override void SetUp()
        {
            base.SetUp();

            _sut = Container.Resolve<IDocumentManager>();
        }
        
        [IdentifiedTestCase("92F8A6B8-EDF5-485C-85BA-E28EDC7A3623", false, 0, 0)]
        [IdentifiedTestCase("B02FDE5D-7AF9-443B-9C2B-624399CCAB4E", true, 10, 10)]
        public void UserPermissionsToGetPercentagePushedToReviewVerification(bool workspaceOrArtifactInstancePermissionsValue,
             int expectedTotalDocuments, int expectedTotalDocumentsPushedToReview)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue);
            PercentagePushedToReviewModel percentagePushedToReviewModel = new PercentagePushedToReviewModel();

            PercentagePushedToReviewRequest percentagePushedToReviewRequest = new PercentagePushedToReviewRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            // Act
            percentagePushedToReviewModel = ActAndGetResult(
                 () => _sut.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest).Result,
                percentagePushedToReviewModel, workspaceOrArtifactInstancePermissionsValue);

            // Assert
            Assert();
            percentagePushedToReviewModel.TotalDocuments.ShouldBeEquivalentTo(expectedTotalDocuments);
            percentagePushedToReviewModel.TotalDocumentsPushedToReview.ShouldBeEquivalentTo(expectedTotalDocumentsPushedToReview);
        }
        
        [IdentifiedTestCase("C411C6B6-13E1-429B-B4CD-3B29CAF9F64C", false, 0, 0, 0, 0)]
        [IdentifiedTestCase("5AC340C0-B186-4CA1-9587-6A6C9F1FBA14", true, 10, 10, 10, 10)]
        public void UserPermissionsToGetCurrentPromotionStatusVerification(bool workspaceOrArtifactInstancePermissionsValue,
            int expectedTotalDocumentsPushedToReview, int expectedTotalDocumentsExcluded,
            int expectedTotalDocumentsIncluded, int expectedTotalDocumentsUntagged)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue);
            CurrentPromotionStatusModel currentPromotionStatusModel = new CurrentPromotionStatusModel();

            CurrentPromotionStatusRequest currentPromotionStatusRequest = new CurrentPromotionStatusRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            // Act
            currentPromotionStatusModel = ActAndGetResult(
                () => _sut.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest).Result,
                currentPromotionStatusModel, workspaceOrArtifactInstancePermissionsValue);

            // Assert
            Assert();
            currentPromotionStatusModel.TotalDocumentsPushedToReview.ShouldBeEquivalentTo(expectedTotalDocumentsPushedToReview);
            currentPromotionStatusModel.TotalDocumentsExcluded.ShouldBeEquivalentTo(expectedTotalDocumentsExcluded);
            currentPromotionStatusModel.TotalDocumentsIncluded.ShouldBeEquivalentTo(expectedTotalDocumentsIncluded);
            currentPromotionStatusModel.TotalDocumentsUntagged.ShouldBeEquivalentTo(expectedTotalDocumentsUntagged);
        }
        
        [IdentifiedTestCase("508002D6-A160-45DE-BE2F-606652F2E924", false, 0, 0, 0, 0)]
        [IdentifiedTestCase("E16BF68B-692B-42C8-90FA-B9F3CFD003B0", true, 10, 10, 10, 10)]
        public void UserPermissionsToGetHistoricalPromotionStatusVerification(bool workspaceOrArtifactInstancePermissionsValue,
            int expectedTotalDocumentsExcluded, int expectedTotalDocumentsIncluded, 
            int expectedTotalDocumentsUntagged, int expectedYear)
        {
            // Arrange
            Arrange(workspaceOrArtifactInstancePermissionsValue);
            HistoricalPromotionStatusSummaryModel historicalPromotionStatusSummaryModel = new HistoricalPromotionStatusSummaryModel
            {
                HistoricalPromotionStatus = new[]
                {
                    new HistoricalPromotionStatusModel
                    {
                        TotalDocumentsExcluded = 0,
                        TotalDocumentsIncluded = 0,
                        TotalDocumentsUntagged = 0,
                        Date = new DateTime(0)
                    }
                }
            };

            HistoricalPromotionStatusRequest historicalPromotionStatusRequest = new HistoricalPromotionStatusRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            // Act
            historicalPromotionStatusSummaryModel = ActAndGetResult(
                () => _sut.GetHistoricalPromotionStatusAsync(historicalPromotionStatusRequest).Result,
                historicalPromotionStatusSummaryModel, workspaceOrArtifactInstancePermissionsValue);

            // Assert
            Assert();
            historicalPromotionStatusSummaryModel.HistoricalPromotionStatus[0].TotalDocumentsExcluded.ShouldBeEquivalentTo(expectedTotalDocumentsExcluded);
            historicalPromotionStatusSummaryModel.HistoricalPromotionStatus[0].TotalDocumentsIncluded.ShouldBeEquivalentTo(expectedTotalDocumentsIncluded);
            historicalPromotionStatusSummaryModel.HistoricalPromotionStatus[0].TotalDocumentsUntagged.ShouldBeEquivalentTo(expectedTotalDocumentsUntagged);
            historicalPromotionStatusSummaryModel.HistoricalPromotionStatus[0].Date.ShouldBeEquivalentTo(new DateTime(expectedYear));
        }
    }
}
