using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Moq;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class DocumentManagerTests : KeplerSecurityTestsBase
    {

        private IDocumentManager _sut;
        private Mock<IDocumentRepository> _documentRepositoryFake;
        //private int _INTEGRATION_POINT_ARTIFACT_ID = 554556;

        public override void SetUp()
        {
            base.SetUp();

            _documentRepositoryFake =
                new Mock<IDocumentRepository>();

            Container.Register(Component.For<IDocumentRepository>()
                .UsingFactoryMethod(_ => _documentRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            _sut = new DocumentManager(_loggerFake.Object, _permissionRepositoryFactoryFake.Object,
                Container);
        }

        [IdentifiedTestCase("52574D7B-8A77-40B2-BDE9-A07CB8599A21", false, false)]
        [IdentifiedTestCase("92F8A6B8-EDF5-485C-85BA-E28EDC7A3623", false, true)]
        [IdentifiedTestCase("B02FDE5D-7AF9-443B-9C2B-624399CCAB4E", true, false)]
        [IdentifiedTestCase("29EFE686-3555-428E-A43B-72ACBE18FB87", true, true)]
        public void UserPermissionsToGetPercentagePushedToReviewVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            PercentagePushedToReviewRequest percentagePushedToReviewRequest = new PercentagePushedToReviewRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            _documentRepositoryFake
                .Setup(x => x.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest))
                .Returns(Task.FromResult(new PercentagePushedToReviewModel()));

            PercentagePushedToReviewModel percentagePushedToReviewModel = new PercentagePushedToReviewModel();

            // Act
            percentagePushedToReviewModel = ActAndGetResult(
                () => _sut.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest).Result,
                percentagePushedToReviewModel);

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

        [IdentifiedTestCase("7D38BC15-3935-4DBE-AF51-77DA8FB822E9", false, false)]
        [IdentifiedTestCase("C411C6B6-13E1-429B-B4CD-3B29CAF9F64C", false, true)]
        [IdentifiedTestCase("5AC340C0-B186-4CA1-9587-6A6C9F1FBA14", true, false)]
        [IdentifiedTestCase("D007357E-02FC-4F8B-96C7-EE0C83C9A0B0", true, true)]
        public void UserPermissionsToGetCurrentPromotionStatusVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            CurrentPromotionStatusRequest currentPromotionStatusRequest = new CurrentPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            _documentRepositoryFake
                .Setup(x => x.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest))
                .Returns(Task.FromResult(new CurrentPromotionStatusModel()));

            CurrentPromotionStatusModel currentPromotionStatusModel = new CurrentPromotionStatusModel();

            // Act
            currentPromotionStatusModel = ActAndGetResult(
                () => _sut.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest).Result,
                currentPromotionStatusModel);

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

        [IdentifiedTestCase("F492B090-53E6-4D08-9B1D-67C8C937B0E0", false, false)]
        [IdentifiedTestCase("508002D6-A160-45DE-BE2F-606652F2E924", false, true)]
        [IdentifiedTestCase("E16BF68B-692B-42C8-90FA-B9F3CFD003B0", true, false)]
        [IdentifiedTestCase("D7F5C77A-ADF0-4BC6-9FBF-BB003C7CECB5", true, true)]
        public void UserPermissionsToGetHistoricalPromotionStatusVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            HistoricalPromotionStatusRequest historicalPromotionStatusRequest = new HistoricalPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            _documentRepositoryFake
                .Setup(x => x.GetHistoricalPromotionStatusAsync(historicalPromotionStatusRequest))
                .Returns(Task.FromResult(new HistoricalPromotionStatusSummaryModel()));

            HistoricalPromotionStatusSummaryModel historicalPromotionStatusSummaryModel = new HistoricalPromotionStatusSummaryModel();

            // Act
            historicalPromotionStatusSummaryModel = ActAndGetResult(
                () => _sut.GetHistoricalPromotionStatusAsync(historicalPromotionStatusRequest).Result,
                historicalPromotionStatusSummaryModel);

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
