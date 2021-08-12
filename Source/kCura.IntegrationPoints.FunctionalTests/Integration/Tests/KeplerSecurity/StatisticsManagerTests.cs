using Castle.MicroKernel.Registration;
using kCura.IntegrationPoints.Data.Statistics;
using Moq;
using Relativity.IntegrationPoints.Services;
using Relativity.Logging;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class StatisticsManagerTests : KeplerSecurityTestsBase
    {
        private IStatisticsManager _sut;

        public override void SetUp()
        {
            base.SetUp();

            Mock<ILog> loggerFake = new Mock<ILog>();

            RegisterStatisticsFake<IDocumentTotalStatistics>();
            RegisterStatisticsFake<INativeTotalStatistics>();
            RegisterStatisticsFake<IImageTotalStatistics>();
            RegisterStatisticsFake<IImageFileSizeStatistics>();
            RegisterStatisticsFake<INativeFileSizeStatistics>();
            
            _sut = new StatisticsManager(loggerFake.Object, _permissionRepositoryFactoryFake.Object, Container);
        }

        [IdentifiedTestCase("6756FA12-4035-4237-9CE8-04EE4E19B436", -1, false)]
        [IdentifiedTestCase("459A51D6-9EFC-48E8-8303-C0892B45A7E2", 1, true)]
        public void UserPermissionsToGetDocumentsTotalForSavedSearchVerification(
            int expectedTotalDocuments, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalDocuments = ActAndGetResult(() => (int)_sut.GetDocumentsTotalForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalDocuments, expectedTotalDocuments, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("BDC4DC5D-5C56-4A4D-8178-483204D37F10", -1, false)]
        [IdentifiedTestCase("52A218C1-0C7B-465D-8983-8BD2F50E6F68", 1, true)]
        public void UserPermissionsToGetNativesTotalForSavedSearchVerification(
            int expectedTotalNatives, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalNatives = ActAndGetResult(() => (int)_sut.GetNativesTotalForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalNatives, expectedTotalNatives, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 1, true)]
        public void UserPermissionsToGetImagesTotalForSavedSearchVerification(
            int expectedTotalImages, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalImages = ActAndGetResult(() => (int)_sut.GetImagesTotalForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalImages, expectedTotalImages, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 1, true)]
        public void UserPermissionsToGetImagesFileSizeForSavedSearchVerification(
            int expectedTotalImages, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalImages = ActAndGetResult(() => (int)_sut.GetImagesFileSizeForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalImages, expectedTotalImages, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 1, true)]
        public void UserPermissionsToGetNativesFileSizeForSavedSearchVerification(
            int expectedTotalNatives, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalNatives = ActAndGetResult(() => (int)_sut.GetNativesFileSizeForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalNatives, expectedTotalNatives, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("6756FA12-4035-4237-9CE8-04EE4E19B436", -1, false)]
        [IdentifiedTestCase("459A51D6-9EFC-48E8-8303-C0892B45A7E2", 1, true)]
        public void UserPermissionsToGetDocumentsTotalForProductionVerification(
            int expectedTotalDocuments, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalDocuments = ActAndGetResult(() => (int)_sut.GetDocumentsTotalForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalDocuments, expectedTotalDocuments, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("BDC4DC5D-5C56-4A4D-8178-483204D37F10", -1, false)]
        [IdentifiedTestCase("52A218C1-0C7B-465D-8983-8BD2F50E6F68", 1, true)]
        public void UserPermissionsToGetNativesTotalForProductionVerification(
           int expectedTotalNatives, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalNatives = ActAndGetResult(() => (int)_sut.GetNativesTotalForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalNatives, expectedTotalNatives, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 1, true)]
        public void UserPermissionsToGetImagesTotalForProductionVerification(
            int expectedTotalImages, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalImages = ActAndGetResult(() => (int)_sut.GetImagesTotalForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalImages, expectedTotalImages, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 1, true)]
        public void UserPermissionsToGetImagesFileSizeForProductionVerification(
            int expectedTotalImages, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalImages = ActAndGetResult(() => (int)_sut.GetImagesFileSizeForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalImages, expectedTotalImages, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("F59F0CC4-FC64-4AD3-9676-2B772E092871", -1, false)]
        [IdentifiedTestCase("723C0BAF-1AB5-4E03-A13D-B8B6E32CA987", 1, true)]
        public void UserPermissionsToGetNativesFileSizeForProductionVerification(
            int expectedTotalNatives, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalNatives = ActAndGetResult(() => (int)_sut.GetNativesFileSizeForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalNatives, expectedTotalNatives, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("3176867D-8A15-4D14-80AD-CC3FF26A8C4D", -1, false)]
        [IdentifiedTestCase("AA32E825-C9D9-4F20-AA82-0B2C56D4B056", 1, true)]
        public void UserPermissionsToGetDocumentsTotalForFolderVerification(
            int expectedTotalDocuments, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalDocuments = ActAndGetResult(() => (int)_sut.GetDocumentsTotalForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalDocuments, expectedTotalDocuments, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("A7C6156F-CE5D-4F6B-A9BC-ABC21A70B879", -1, false)]
        [IdentifiedTestCase("1265E9CF-15C6-4AB5-A35D-620FA6331C98", 1, true)]
        public void UserPermissionsToGetNativesTotalForFolderVerification(
            int expectedTotalNatives, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalNatives = ActAndGetResult(() => (int)_sut.GetNativesTotalForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalNatives, expectedTotalNatives, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 1, true)]
        public void UserPermissionsToGetImagesTotalForFolderVerification(
            int expectedTotalImages, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalImages = ActAndGetResult(() => (int)_sut.GetImagesTotalForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalImages, expectedTotalImages, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("A7DAF9BF-2CC4-486D-A20A-6A74270CD678", -1, false)]
        [IdentifiedTestCase("08B92ECD-74BC-462A-8BB7-2391BDBE9CB5", 1, true)]
        public void UserPermissionsToGetImagesFileSizeForFolderVerification(
            int expectedTotalImages, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalImages = ActAndGetResult(() => (int)_sut.GetImagesFileSizeForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalImages, expectedTotalImages, expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("B4754349-31CB-4412-8AAC-B36D8105F69D", -1, false)]
        [IdentifiedTestCase("D7E4C96E-4775-4AE9-8DA2-1B2AC476BD2D", 1, true)]
        public void UserPermissionsToGetNativesFileSizeForFolderVerification(
            int expectedTotalNatives, bool workspaceAccessPermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions);

            // Act
            int totalNatives = ActAndGetResult(() => (int)_sut.GetNativesFileSizeForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions
            };

            // Assert
            Assert(totalNatives, expectedTotalNatives, expectedRepositoryPermissions);
        }

        private void RegisterStatisticsFake<T>() where T : class, IDocumentStatistics
        {
            Mock<T> documentTotalStatisticsFake = new Mock<T>();
            documentTotalStatisticsFake.Setup(x => x.ForSavedSearch(_WORKSPACE_ID, _SAVEDSEARCH_ID))
                .Returns(1);
            documentTotalStatisticsFake.Setup(x => x.ForProduction(_WORKSPACE_ID, _SAVEDSEARCH_ID))
                .Returns(1);
            documentTotalStatisticsFake.Setup(x => x.ForFolder(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false))
                .Returns(1);

            Container.Register(Component.For<T>().UsingFactoryMethod(k => documentTotalStatisticsFake.Object).LifestyleTransient().IsDefault());
        }
    }
}
