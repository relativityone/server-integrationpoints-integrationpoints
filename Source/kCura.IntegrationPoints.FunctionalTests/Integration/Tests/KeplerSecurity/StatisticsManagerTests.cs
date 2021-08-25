using FluentAssertions;
using Relativity.IntegrationPoints.Services;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class StatisticsManagerTests : KeplerSecurityTestsBase
    {
        private readonly int _savedSearchId = ArtifactProvider.NextId();
        private readonly int _viewId = ArtifactProvider.NextId();

        private IStatisticsManager _sut;

        public override void SetUp()
        {
            base.SetUp();

            _sut = new StatisticsManager(Logger, PermissionRepositoryFactory, Container);
        }

        [IdentifiedTestCase("6756FA12-4035-4237-9CE8-04EE4E19B436", -1, false)]
        [IdentifiedTestCase("459A51D6-9EFC-48E8-8303-C0892B45A7E2", 10L, true)]
        public void UserPermissionsToGetDocumentsTotalForSavedSearchVerification(
            long expectedTotalDocuments, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            long totalDocuments = -1;

            // Act
            totalDocuments = ActAndGetResult(() => _sut.GetDocumentsTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalDocuments, workspaceAccessPermissionsValue);

            // Assert
            Assert();
            totalDocuments.ShouldBeEquivalentTo(expectedTotalDocuments);
        }

        [IdentifiedTestCase("BDC4DC5D-5C56-4A4D-8178-483204D37F10", -1, false)]
        [IdentifiedTestCase("52A218C1-0C7B-465D-8983-8BD2F50E6F68", 10L, true)]
        public void UserPermissionsToGetNativesTotalForSavedSearchVerification(
            long expectedTotalNatives, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalNatives = -1;

            // Act
            totalNatives = ActAndGetResult(() => (int)_sut.GetNativesTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalNatives, workspaceAccessPermissionsValue);

            // Assert
            Assert();
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 10L, true)]
        public void UserPermissionsToGetImagesTotalForSavedSearchVerification(
            long expectedTotalImages, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalImages = -1;

            // Act
            totalImages = ActAndGetResult(() => (int)_sut.GetImagesTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalImages, workspaceAccessPermissionsValue);

            // Assert
            Assert();
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 10L, true)]
        public void UserPermissionsToGetImagesFileSizeForSavedSearchVerification(
            long expectedTotalImages, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalImages = -1;

            // Act
            totalImages = ActAndGetResult(() => (int)_sut.GetImagesFileSizeForSavedSearchAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalImages, workspaceAccessPermissionsValue);

            // Assert
            Assert();
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 10L, true)]
        public void UserPermissionsToGetNativesFileSizeForSavedSearchVerification(
            long expectedTotalNatives, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalNatives = -1;

            // Act
            totalNatives = ActAndGetResult(() => (int)_sut.GetNativesFileSizeForSavedSearchAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalNatives, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
        }

        [IdentifiedTestCase("6756FA12-4035-4237-9CE8-04EE4E19B436", -1, false)]
        [IdentifiedTestCase("459A51D6-9EFC-48E8-8303-C0892B45A7E2", 10L, true)]
        public void UserPermissionsToGetDocumentsTotalForProductionVerification(
            long expectedTotalDocuments, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalDocuments = -1;

            // Act
            totalDocuments = ActAndGetResult(() => (int)_sut.GetDocumentsTotalForProductionAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalDocuments, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalDocuments.ShouldBeEquivalentTo(expectedTotalDocuments);
        }

        [IdentifiedTestCase("BDC4DC5D-5C56-4A4D-8178-483204D37F10", -1, false)]
        [IdentifiedTestCase("52A218C1-0C7B-465D-8983-8BD2F50E6F68", 10L, true)]
        public void UserPermissionsToGetNativesTotalForProductionVerification(
           long expectedTotalNatives, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalNatives = -1;

            // Act
            totalNatives = ActAndGetResult(() => (int)_sut.GetNativesTotalForProductionAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalNatives, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 10L, true)]
        public void UserPermissionsToGetImagesTotalForProductionVerification(
            long expectedTotalImages, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalImages = -1;

            // Act
            totalImages = ActAndGetResult(() => (int)_sut.GetImagesTotalForProductionAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalImages, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 10L, true)]
        public void UserPermissionsToGetImagesFileSizeForProductionVerification(
            long expectedTotalImages, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalImages = -1;

            // Act
            totalImages = ActAndGetResult(() => (int)_sut.GetImagesFileSizeForProductionAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalImages, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
        }

        [IdentifiedTestCase("F59F0CC4-FC64-4AD3-9676-2B772E092871", -1, false)]
        [IdentifiedTestCase("723C0BAF-1AB5-4E03-A13D-B8B6E32CA987", 10L, true)]
        public void UserPermissionsToGetNativesFileSizeForProductionVerification(
            long expectedTotalNatives, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalNatives = -1;

            // Act
            totalNatives = ActAndGetResult(() => (int)_sut.GetNativesFileSizeForProductionAsync(SourceWorkspace.ArtifactId, _savedSearchId).Result,
                totalNatives, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
        }

        [IdentifiedTestCase("3176867D-8A15-4D14-80AD-CC3FF26A8C4D", -1, false)]
        [IdentifiedTestCase("AA32E825-C9D9-4F20-AA82-0B2C56D4B056", 10L, true)]
        public void UserPermissionsToGetDocumentsTotalForFolderVerification(
            long expectedTotalDocuments, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalDocuments = -1;

            // Act
            totalDocuments = ActAndGetResult(() => (int)_sut.GetDocumentsTotalForFolderAsync(SourceWorkspace.ArtifactId, _savedSearchId, _viewId, false).Result,
                totalDocuments, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalDocuments.ShouldBeEquivalentTo(expectedTotalDocuments);
        }

        [IdentifiedTestCase("A7C6156F-CE5D-4F6B-A9BC-ABC21A70B879", -1, false)]
        [IdentifiedTestCase("1265E9CF-15C6-4AB5-A35D-620FA6331C98", 10L, true)]
        public void UserPermissionsToGetNativesTotalForFolderVerification(
            long expectedTotalNatives, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalNatives = -1;

            // Act
            totalNatives = ActAndGetResult(() => (int)_sut.GetNativesTotalForFolderAsync(SourceWorkspace.ArtifactId, _savedSearchId, _viewId, false).Result,
                totalNatives, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 10L, true)]
        public void UserPermissionsToGetImagesTotalForFolderVerification(
            long expectedTotalImages, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalImages = -1;

            // Act
            totalImages = ActAndGetResult(() => (int)_sut.GetImagesTotalForFolderAsync(SourceWorkspace.ArtifactId, _savedSearchId, _viewId, false).Result,
                totalImages, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
        }

        [IdentifiedTestCase("A7DAF9BF-2CC4-486D-A20A-6A74270CD678", -1, false)]
        [IdentifiedTestCase("08B92ECD-74BC-462A-8BB7-2391BDBE9CB5", 10L, true)]
        public void UserPermissionsToGetImagesFileSizeForFolderVerification(
            long expectedTotalImages, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalImages = -1;

            // Act
            totalImages = ActAndGetResult(() => (int)_sut.GetImagesFileSizeForFolderAsync(SourceWorkspace.ArtifactId, _savedSearchId, _viewId, false).Result,
                totalImages, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
        }

        [IdentifiedTestCase("B4754349-31CB-4412-8AAC-B36D8105F69D", -1, false)]
        [IdentifiedTestCase("D7E4C96E-4775-4AE9-8DA2-1B2AC476BD2D", 10L, true)]
        public void UserPermissionsToGetNativesFileSizeForFolderVerification(
            long expectedTotalNatives, bool workspaceAccessPermissionsValue)
        {
            // Arrange
            Arrange(workspaceAccessPermissionsValue);
            int totalNatives = -1;

            // Act
            totalNatives = ActAndGetResult(() => (int)_sut.GetNativesFileSizeForFolderAsync(SourceWorkspace.ArtifactId, _savedSearchId, _viewId, false).Result,
                totalNatives, workspaceAccessPermissionsValue);
            
            // Assert
            Assert();
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
        }
    }
}
