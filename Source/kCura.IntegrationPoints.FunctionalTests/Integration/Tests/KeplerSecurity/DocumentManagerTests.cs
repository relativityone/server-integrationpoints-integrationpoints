using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class DocumentManagerTests : KeplerSecurityTestsBase
    {
        private IDocumentManager _sut;
        private Mock<IDocumentRepository> _documentRepositoryFake;

        public override void SetUp()
        {
            base.SetUp();

            _documentRepositoryFake =
                new Mock<IDocumentRepository>();

            Container.Register(Component.For<IDocumentRepository>()
                .UsingFactoryMethod(_ => _documentRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            _sut = Container.Resolve<IDocumentManager>();
        }
        
        [IdentifiedTestCase("92F8A6B8-EDF5-485C-85BA-E28EDC7A3623", false, 0, 0)]
        [IdentifiedTestCase("B02FDE5D-7AF9-443B-9C2B-624399CCAB4E", true, 10, 20)]
        public void UserPermissionsToGetPercentagePushedToReviewVerification(bool workspaceAccessPermissions,
             int expectedTotalDocuments, int expectedTotalDocumentsPushedToReview)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);
            PercentagePushedToReviewModel percentagePushedToReviewModel = new PercentagePushedToReviewModel();

            PercentagePushedToReviewRequest percentagePushedToReviewRequest = new PercentagePushedToReviewRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            PercentagePushedToReviewModel expectedPercentagePushedToReviewModel = new PercentagePushedToReviewModel
            {
                TotalDocumentsPushedToReview = expectedTotalDocumentsPushedToReview,
                TotalDocuments = expectedTotalDocuments
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            _documentRepositoryFake
                .Setup(x => x.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest))
                .Returns(Task.FromResult(expectedPercentagePushedToReviewModel));
            
            // Act
            percentagePushedToReviewModel = ActAndGetResult(
                 () => _sut.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest).Result,
                percentagePushedToReviewModel, workspaceAccessPermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            percentagePushedToReviewModel.TotalDocuments.ShouldBeEquivalentTo(expectedTotalDocuments);
            percentagePushedToReviewModel.TotalDocumentsPushedToReview.ShouldBeEquivalentTo(expectedTotalDocumentsPushedToReview);
        }
        
        [IdentifiedTestCase("C411C6B6-13E1-429B-B4CD-3B29CAF9F64C", false, 0, 0, 0, 0)]
        [IdentifiedTestCase("5AC340C0-B186-4CA1-9587-6A6C9F1FBA14", true, 10, 20, 30, 40)]
        public void UserPermissionsToGetCurrentPromotionStatusVerification(bool workspaceAccessPermissions,
            int expectedTotalDocumentsPushedToReview, int expectedTotalDocumentsExcluded,
            int expectedTotalDocumentsIncluded, int expectedTotalDocumentsUntagged)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);
            CurrentPromotionStatusModel currentPromotionStatusModel = new CurrentPromotionStatusModel();

            CurrentPromotionStatusRequest currentPromotionStatusRequest = new CurrentPromotionStatusRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            CurrentPromotionStatusModel expectedCurrentPromotionStatusModel = new CurrentPromotionStatusModel
            {
                TotalDocumentsPushedToReview = expectedTotalDocumentsPushedToReview,
                TotalDocumentsExcluded = expectedTotalDocumentsExcluded,
                TotalDocumentsIncluded = expectedTotalDocumentsIncluded,
                TotalDocumentsUntagged = expectedTotalDocumentsUntagged
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            _documentRepositoryFake
                .Setup(x => x.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest))
                .Returns(Task.FromResult(expectedCurrentPromotionStatusModel));

            // Act
            currentPromotionStatusModel = ActAndGetResult(
                () => _sut.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest).Result,
                currentPromotionStatusModel, workspaceAccessPermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            currentPromotionStatusModel.TotalDocumentsPushedToReview.ShouldBeEquivalentTo(expectedTotalDocumentsPushedToReview);
            currentPromotionStatusModel.TotalDocumentsExcluded.ShouldBeEquivalentTo(expectedTotalDocumentsExcluded);
            currentPromotionStatusModel.TotalDocumentsIncluded.ShouldBeEquivalentTo(expectedTotalDocumentsIncluded);
            currentPromotionStatusModel.TotalDocumentsUntagged.ShouldBeEquivalentTo(expectedTotalDocumentsUntagged);
        }
        
        [IdentifiedTestCase("508002D6-A160-45DE-BE2F-606652F2E924", false, 0, 0, 0, 0)]
        [IdentifiedTestCase("E16BF68B-692B-42C8-90FA-B9F3CFD003B0", true, 10, 20, 30, 40)]
        public void UserPermissionsToGetHistoricalPromotionStatusVerification(bool workspaceAccessPermissions,
            int expectedTotalDocumentsExcluded, int expectedTotalDocumentsIncluded, 
            int expectedTotalDocumentsUntagged, int expectedYear)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);
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

            HistoricalPromotionStatusSummaryModel expectedHistoricalPromotionStatusSummaryModel =
                new HistoricalPromotionStatusSummaryModel
                {
                    HistoricalPromotionStatus = new []
                    {
                        new HistoricalPromotionStatusModel
                        {
                            TotalDocumentsExcluded = expectedTotalDocumentsExcluded,
                            TotalDocumentsIncluded = expectedTotalDocumentsIncluded,
                            TotalDocumentsUntagged = expectedTotalDocumentsUntagged,
                            Date = new DateTime(expectedYear)
                        }
                    }
                };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            _documentRepositoryFake
                .Setup(x => x.GetHistoricalPromotionStatusAsync(historicalPromotionStatusRequest))
                .Returns(Task.FromResult(expectedHistoricalPromotionStatusSummaryModel));

            // Act
            historicalPromotionStatusSummaryModel = ActAndGetResult(
                () => _sut.GetHistoricalPromotionStatusAsync(historicalPromotionStatusRequest).Result,
                historicalPromotionStatusSummaryModel, workspaceAccessPermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            historicalPromotionStatusSummaryModel.HistoricalPromotionStatus[0].TotalDocumentsExcluded.ShouldBeEquivalentTo(expectedTotalDocumentsExcluded);
            historicalPromotionStatusSummaryModel.HistoricalPromotionStatus[0].TotalDocumentsIncluded.ShouldBeEquivalentTo(expectedTotalDocumentsIncluded);
            historicalPromotionStatusSummaryModel.HistoricalPromotionStatus[0].TotalDocumentsUntagged.ShouldBeEquivalentTo(expectedTotalDocumentsUntagged);
            historicalPromotionStatusSummaryModel.HistoricalPromotionStatus[0].Date.ShouldBeEquivalentTo(new DateTime(expectedYear));
        }
    }
}
