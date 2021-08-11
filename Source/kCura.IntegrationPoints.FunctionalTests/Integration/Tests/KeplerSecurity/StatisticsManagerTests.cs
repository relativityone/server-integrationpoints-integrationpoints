using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Statistics;
using Moq;
using Relativity.API;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Logging;
using Relativity.Services.Permission;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class StatisticsManagerTests : TestsBase
    {
        private const int _WORKSPACE_ID = 266818;
        private const int _SAVEDSEARCH_ID = 4324;
        private const int _VIEW_ID = 1234;

        private IStatisticsManager _sut;
        private IPermissionRepository _permissionRepository;

        private Mock<IPermissionManager> _permissionManagerFake;

        public override void SetUp()
        {
            base.SetUp();
            _permissionRepository = new PermissionRepository(Helper, _WORKSPACE_ID);

            Mock<ILog> loggerFake = new Mock<ILog>();
            Mock<IPermissionRepositoryFactory> permissionRepositoryFactoryFake = new Mock<IPermissionRepositoryFactory>();

            _permissionManagerFake = new Mock<IPermissionManager>();
            permissionRepositoryFactoryFake.Setup(x => x.Create(null, _WORKSPACE_ID))
                .Returns(_permissionRepository);

            Mock<IServicesMgr> serviceManagerFake = Helper.GetServicesManagerMock();
            serviceManagerFake.Setup(x => x.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
                .Returns(_permissionManagerFake.Object);

            RegisterStatisticsFake<IDocumentTotalStatistics>();
            RegisterStatisticsFake<INativeTotalStatistics>();
            RegisterStatisticsFake<IImageTotalStatistics>();
            RegisterStatisticsFake<IImageFileSizeStatistics>();
            RegisterStatisticsFake<INativeFileSizeStatistics>();
            
            _sut = new StatisticsManager(loggerFake.Object, permissionRepositoryFactoryFake.Object, Container);
        }

        [IdentifiedTestCase("6756FA12-4035-4237-9CE8-04EE4E19B436", -1L, false)]
        [IdentifiedTestCase("459A51D6-9EFC-48E8-8303-C0892B45A7E2", 1L, true)]
        public void UserPermissionsToGetDocumentsTotalForSavedSearchVerification(
            long expectedTotalDocuments, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalDocuments = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalDocuments = _sut.GetDocumentsTotalForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalDocuments.ShouldBeEquivalentTo(expectedTotalDocuments);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("BDC4DC5D-5C56-4A4D-8178-483204D37F10", -1L, false)]
        [IdentifiedTestCase("52A218C1-0C7B-465D-8983-8BD2F50E6F68", 1L, true)]
        public void UserPermissionsToGetNativesTotalForSavedSearchVerification(
            long expectedTotalNatives, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalNatives = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalNatives = _sut.GetNativesTotalForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1L, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 1L, true)]
        public void UserPermissionsToGetImagesTotalForSavedSearchVerification(
            long expectedTotalImages, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalImages = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalImages = _sut.GetImagesTotalForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1L, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 1L, true)]
        public void UserPermissionsToGetImagesFileSizeForSavedSearchVerification(
            long expectedTotalImages, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalImages = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalImages = _sut.GetImagesFileSizeForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1L, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 1L, true)]
        public void UserPermissionsToGetNativesFileSizeForSavedSearchVerification(
            long expectedTotalNatives, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalNatives = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalNatives = _sut.GetNativesFileSizeForSavedSearchAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("6756FA12-4035-4237-9CE8-04EE4E19B436", -1L, false)]
        [IdentifiedTestCase("459A51D6-9EFC-48E8-8303-C0892B45A7E2", 1L, true)]
        public void UserPermissionsToGetDocumentsTotalForProductionVerification(
            long expectedTotalDocuments, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalDocuments = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalDocuments = _sut.GetDocumentsTotalForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalDocuments.ShouldBeEquivalentTo(expectedTotalDocuments);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("BDC4DC5D-5C56-4A4D-8178-483204D37F10", -1L, false)]
        [IdentifiedTestCase("52A218C1-0C7B-465D-8983-8BD2F50E6F68", 1L, true)]
        public void UserPermissionsToGetNativesTotalForProductionVerification(
           long expectedTotalNatives, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalNatives = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalNatives = _sut.GetNativesTotalForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1L, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 1L, true)]
        public void UserPermissionsToGetImagesTotalForProductionVerification(
            long expectedTotalImages, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalImages = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalImages = _sut.GetImagesTotalForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("7AEFDACD-78E5-45F9-8830-59BD9D4D9383", -1L, false)]
        [IdentifiedTestCase("4224CC0D-B862-4966-BD1E-FD9422D91278", 1L, true)]
        public void UserPermissionsToGetImagesFileSizeForProductionVerification(
            long expectedTotalImages, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalImages = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalImages = _sut.GetImagesFileSizeForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("F59F0CC4-FC64-4AD3-9676-2B772E092871", -1L, false)]
        [IdentifiedTestCase("723C0BAF-1AB5-4E03-A13D-B8B6E32CA987", 1L, true)]
        public void UserPermissionsToGetNativesFileSizeForProductionVerification(
            long expectedTotalNatives, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalNatives = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalNatives = _sut.GetNativesFileSizeForProductionAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("3176867D-8A15-4D14-80AD-CC3FF26A8C4D", -1L, false)]
        [IdentifiedTestCase("AA32E825-C9D9-4F20-AA82-0B2C56D4B056", 1L, true)]
        public void UserPermissionsToGetDocumentsTotalForFolderVerification(
            long expectedTotalDocuments, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalDocuments = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalDocuments = _sut.GetDocumentsTotalForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalDocuments.ShouldBeEquivalentTo(expectedTotalDocuments);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("A7C6156F-CE5D-4F6B-A9BC-ABC21A70B879", -1L, false)]
        [IdentifiedTestCase("1265E9CF-15C6-4AB5-A35D-620FA6331C98", 1L, true)]
        public void UserPermissionsToGetNativesTotalForFolderVerification(
            long expectedTotalNatives, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalNatives = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalNatives = _sut.GetNativesTotalForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("019B1B5D-FBC5-44B3-B876-AA9245443F07", -1L, false)]
        [IdentifiedTestCase("B551A3E5-7A5D-4741-96DC-58A8B5E9FFE4", 1L, true)]
        public void UserPermissionsToGetImagesTotalForFolderVerification(
            long expectedTotalImages, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalImages = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalImages = _sut.GetImagesTotalForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("A7DAF9BF-2CC4-486D-A20A-6A74270CD678", -1L, false)]
        [IdentifiedTestCase("08B92ECD-74BC-462A-8BB7-2391BDBE9CB5", 1L, true)]
        public void UserPermissionsToGetImagesFileSizeForFolderVerification(
            long expectedTotalImages, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalImages = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalImages = _sut.GetImagesFileSizeForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalImages.ShouldBeEquivalentTo(expectedTotalImages);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
        }

        [IdentifiedTestCase("B4754349-31CB-4412-8AAC-B36D8105F69D", -1L, false)]
        [IdentifiedTestCase("D7E4C96E-4775-4AE9-8DA2-1B2AC476BD2D", 1L, true)]
        public void UserPermissionsToGetNativesFileSizeForFolderVerification(
            long expectedTotalNatives, bool userShouldHasArtifactTypePermissions)
        {
            // Arrange
            long totalNatives = -1L;
            GetPermissionsSelectedAsyncFake(userShouldHasArtifactTypePermissions);

            // Act
            try
            {
                totalNatives = _sut.GetNativesFileSizeForFolderAsync(_WORKSPACE_ID, _SAVEDSEARCH_ID, _VIEW_ID, false).Result;
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InsufficientPermissionException>();
            }

            bool userHasArtifactTypePermissions = _permissionRepository.UserHasPermissionToAccessWorkspace();

            // Assert
            totalNatives.ShouldBeEquivalentTo(expectedTotalNatives);
            userHasArtifactTypePermissions.ShouldBeEquivalentTo(userShouldHasArtifactTypePermissions);
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

        private void GetPermissionsSelectedAsyncFake(bool permissionValue)
        {
            List<PermissionRef> permissions = new List<PermissionRef> { new PermissionRef() };
            List<PermissionValue> permissionValues = new List<PermissionValue>
            {
                new PermissionValue
                {
                    Selected = permissionValue
                }
            };

            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(-1,
                permissions, _WORKSPACE_ID)).Returns(Task.FromResult(permissionValues));
        }
    }
}
