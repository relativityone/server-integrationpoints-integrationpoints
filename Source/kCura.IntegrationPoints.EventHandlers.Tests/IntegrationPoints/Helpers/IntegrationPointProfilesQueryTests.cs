using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointProfilesQueryTests
    {
        private IntegrationPointProfilesQuery _sut;
        private Mock<IRelativityObjectManager> _relativityObjectManagerMock;
        private Mock<IObjectArtifactIdsByStringFieldValueQuery> _objectArtifactIDsQueryFake;
        private ISerializer _serializer;
        private List<IntegrationPointProfile> _profilesToUpdate;
        private List<IntegrationPointProfile> _profilesToDelete;
        private List<IntegrationPointProfile> _allProfiles;
        private List<int> _relativitySourceProviders;
        private List<int> _relativityDestinationProviders;
        private List<int> _integrationPointTypesList;
        private const int _WORKSPACE_ID = 100111;

        private const int _RELATIVITY_DESTINATION_PROVIDER_ID = 500111;
        private const int _RELATIVITY_SOURCE_PROVIDER_ID = 500222;
        private const int _NON_RELATIVITY_DESTINATION_PROVIDER_ID = 600111;
        private const int _NON_RELATIVITY_SOURCE_PROVIDER_ID = 600222;
        private const int _INTEGRATION_POINT_EXPORT_TYPE_ID = 7000333;

        [SetUp]
        public void SetUp()
        {
            _relativityObjectManagerMock = new Mock<IRelativityObjectManager>();
            _objectArtifactIDsQueryFake = new Mock<IObjectArtifactIdsByStringFieldValueQuery>();
            _serializer = new JSONSerializer();

            _profilesToUpdate = CreateProfilesToUpdate().ToList();
            _profilesToDelete = CreateProfilesToDelete().ToList();
            _allProfiles = new List<IntegrationPointProfile>();
            _allProfiles.AddRange(_profilesToUpdate);
            _allProfiles.AddRange(_profilesToDelete);

            _relativityObjectManagerMock
                .Setup(x => x.QueryAsync<IntegrationPointProfile>(
                    It.IsAny<QueryRequest>(), false, It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(_allProfiles);
            
            _relativityObjectManagerMock
                .Setup(x => x.StreamUnicodeLongText(It.IsAny<int>(), It.IsAny<FieldRef>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(() => new MemoryStream(Encoding.Unicode.GetBytes("{}")));

            _relativitySourceProviders = new List<int>();
            _relativityDestinationProviders = new List<int>();
            _integrationPointTypesList = new List<int>();

            _objectArtifactIDsQueryFake
                .Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
                    (DestinationProvider provider) => provider.Identifier,
                    Constants.IntegrationPoints.DestinationProviders.RELATIVITY))
                .ReturnsAsync(_relativityDestinationProviders);

            _objectArtifactIDsQueryFake
                .Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
                    (SourceProvider provider) => provider.Identifier,
                    Constants.IntegrationPoints.SourceProviders.RELATIVITY))
                .ReturnsAsync(_relativitySourceProviders);

            _objectArtifactIDsQueryFake
                .Setup(x => x.QueryForObjectArtifactIdsByStringFieldValueAsync(_WORKSPACE_ID,
                    (IntegrationPointType integrationPointType) => integrationPointType.Identifier,
                    kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()))
                .ReturnsAsync(_integrationPointTypesList);

            _sut = new IntegrationPointProfilesQuery(
                workspaceID => _relativityObjectManagerMock.Object,
                _objectArtifactIDsQueryFake.Object);
        }

        [Test]
        public async Task GetAllProfilesAsync_ShouldReturnAllProfiles()
        {
            // Arrange
            SetUpSyncProviders();

            // Act
            List<IntegrationPointProfile> allProfiles = (await _sut
                .GetAllProfilesAsync(_WORKSPACE_ID)
                .ConfigureAwait(false)).ToList();

            // Assert
            CollectionAssert.AreEquivalent(_allProfiles, allProfiles);
            _relativityObjectManagerMock.Verify(
                x => x.StreamUnicodeLongText(It.IsAny<int>(), It.Is<FieldRef>(fieldRef => fieldRef.Guid == IntegrationPointProfileFieldGuids.SourceConfigurationGuid), It.IsAny<ExecutionIdentity>()),
                Times.Exactly(_allProfiles.Count));
            _relativityObjectManagerMock.Verify(
                x => x.StreamUnicodeLongText(It.IsAny<int>(), It.Is<FieldRef>(fieldRef => fieldRef.Guid == IntegrationPointProfileFieldGuids.DestinationConfigurationGuid), It.IsAny<ExecutionIdentity>()),
                Times.Exactly(_allProfiles.Count));
        }

        [Test]
        public void GetProfilesToUpdate_ShouldReturnOnlyProfilesToUpdate()
        {
            // Arrange
            SetUpSyncProviders();

            // Act
            IEnumerable<IntegrationPointProfile> syncProfiles = _sut
                .GetProfilesToUpdate(_allProfiles, _RELATIVITY_SOURCE_PROVIDER_ID, _RELATIVITY_DESTINATION_PROVIDER_ID);

            // Assert
            CollectionAssert.AreEquivalent(_profilesToUpdate, syncProfiles);
        }

        [Test]
        public void GetProfilesToDelete_ShouldReturnOnlyProfilesToDelete()
        {
            // Arrange
            SetUpSyncProviders();

            // Act
            IEnumerable<IntegrationPointProfile> nonSyncProfiles = _sut
                .GetProfilesToDelete(_allProfiles, _RELATIVITY_SOURCE_PROVIDER_ID, _RELATIVITY_DESTINATION_PROVIDER_ID);

            // Assert
            CollectionAssert.AreEquivalent(_profilesToDelete, nonSyncProfiles);
        }

        [Test]
        public async Task GetSyncSourceProviderArtifactIDAsync_ShouldReturnProperArtifactID()
        {
            // Arrange
            SetUpSyncProviders();

            // Act
            int sourceProviderID = await _sut.GetSyncSourceProviderArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

            // Assert
            sourceProviderID.Should().Be(_RELATIVITY_SOURCE_PROVIDER_ID);
        }

        [Test]
        public async Task GetIntegrationPointExportTypeArtifactIDAsync_ShouldReturnProperArtifactID()
        {
            // Arrange
            SetUpSyncProviders();

            // Act
            int sourceProviderID = await _sut.GetIntegrationPointExportTypeArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

            // Assert
            sourceProviderID.Should().Be(_INTEGRATION_POINT_EXPORT_TYPE_ID);
        }

        [Test]
        public async Task GetSyncDestinationProviderArtifactIDAsync_ShouldReturnNonSyncSourceProviderArtifactID()
        {
            // Arrange
            SetUpSyncProviders();

            // Act
            int destinationProviderArtifactID = await _sut.GetSyncDestinationProviderArtifactIDAsync(_WORKSPACE_ID).ConfigureAwait(false);

            // Assert
            destinationProviderArtifactID.Should().Be(_RELATIVITY_DESTINATION_PROVIDER_ID);
        }

        [Test]
        public void GetIntegrationPointExportTypeArtifactIDAsync_ShouldFail_WhenNoIntegrationPointExportTypeArtifactID()
        {
            // Arrange
            SetUpSyncProviders(integrationPointTypesCount: 0);

            // Act
            Func<Task<int>> action = () => _sut.GetIntegrationPointExportTypeArtifactIDAsync(_WORKSPACE_ID);

            // Assert
            AssertNoArtifactIDsInCollection(action);
        }

        [Test]
        public void GetSyncSourceProviderArtifactIDAsync_ShouldFail_WhenNoSourceProviders()
        {
            // Arrange
            SetUpSyncProviders(relativitySourceProviderCount: 0);

            // Act
            Func<Task<int>> action = () => _sut.GetSyncSourceProviderArtifactIDAsync(_WORKSPACE_ID);

            // Assert
            AssertNoArtifactIDsInCollection(action);
        }

        [Test]
        public void GetSyncDestinationProviderArtifactIDAsync_ShouldFail_WhenNoDestinationProviders()
        {
            // Arrange
            SetUpSyncProviders(relativityDestinationProviderCount: 0);

            // Act
            Func<Task<int>> action = () => _sut.GetSyncDestinationProviderArtifactIDAsync(_WORKSPACE_ID);

            // Assert
            AssertNoArtifactIDsInCollection(action);
        }
        
        [Test]
        public void GetIntegrationPointExportTypeArtifactIDAsync_ShouldFail_WhenOneOrMoreIntegrationPointExportTypeArtifactID([Values(1, 2)] int integrationPointTypesCount)
        {
            // Arrange
            SetUpSyncProviders(integrationPointTypesCount: integrationPointTypesCount);

            // Act
            Func<Task<int>> action = () => _sut.GetIntegrationPointExportTypeArtifactIDAsync(_WORKSPACE_ID);

            // Assert
            AssertOneOrMoreArtifactIDsInCollection(action);
        }

        [Test]
        public void GetSyncSourceProviderArtifactIDAsync_ShouldFail_WhenOneOrMoreSourceProviders([Values(1, 2)] int relativitySourceProviderCount)
        {
            // Arrange
            SetUpSyncProviders(relativitySourceProviderCount);

            // Act
            Func<Task<int>> action = () => _sut.GetSyncSourceProviderArtifactIDAsync(_WORKSPACE_ID);

            // Assert
            AssertOneOrMoreArtifactIDsInCollection(action);
        }

        [Test]
        public void GetSyncDestinationProviderArtifactIDAsync_ShouldFail_WhenOneOrMoreDestinationProviders([Values(1, 2)] int relativityDestinationProviderCount)
        {
            // Arrange
            SetUpSyncProviders(relativityDestinationProviderCount: relativityDestinationProviderCount);

            // Act
            Func<Task<int>> action = () => _sut.GetSyncDestinationProviderArtifactIDAsync(_WORKSPACE_ID);

            // Assert
            AssertOneOrMoreArtifactIDsInCollection(action);
        }

        private static void AssertNoArtifactIDsInCollection(Func<Task<int>> sut)
        {
            sut.ShouldThrowExactly<InvalidOperationException>();
        }

        private static void AssertOneOrMoreArtifactIDsInCollection(Func<Task<int>> sut)
        {
            sut.ShouldNotThrow<InvalidOperationException>();
        }

        private IEnumerable<IntegrationPointProfile> CreateProfilesToUpdate()
        {
            yield return new IntegrationPointProfile
            {
                SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
                DestinationConfiguration = CreateDestinationConfigurationJson(false)
            };
            yield return new IntegrationPointProfile
            {
                SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
                DestinationConfiguration = CreateDestinationConfigurationJson(false, true, true)
            };

        }

        private IEnumerable<IntegrationPointProfile> CreateProfilesToDelete()
        {
            yield return new IntegrationPointProfile()
            {
                SourceProvider = _NON_RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _NON_RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.ProductionSet),
                DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: true)
            };
            yield return new IntegrationPointProfile()
            {
                SourceProvider = _NON_RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _NON_RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
                DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: false)
            };
            yield return new IntegrationPointProfile()
            {
                SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _NON_RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
                DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: false)
            };
            yield return new IntegrationPointProfile()
            {
                SourceProvider = _NON_RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
                DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: false)
            };
            yield return new IntegrationPointProfile()
            {
                SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.ProductionSet),
                DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: true)
            };
            yield return new IntegrationPointProfile()
            {
                SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
                DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: true)
            };
            yield return new IntegrationPointProfile()
            {
                SourceProvider = _RELATIVITY_SOURCE_PROVIDER_ID,
                DestinationProvider = _RELATIVITY_DESTINATION_PROVIDER_ID,
                SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.ProductionSet),
                DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: false)
            };
        }

        private string CreateSourceConfigurationJson(SourceConfiguration.ExportType exportType)
        {
            return _serializer.Serialize(new SourceConfiguration()
            {
                TypeOfExport = exportType
            });
        }

        private string CreateDestinationConfigurationJson(bool exportToProduction = false, bool copyImages = false, bool useImagePrecedence = false)
        {
            var importSettings = new ImportSettings()
            {
                ProductionImport = exportToProduction,
                ImageImport = copyImages
            };
            if (useImagePrecedence)
            {
                importSettings.ImagePrecedence = new List<ProductionDTO>()
                {
                    new ProductionDTO()
                };
            }
            return _serializer.Serialize(importSettings);
        }

        private void SetUpSyncProviders(int relativitySourceProviderCount = 1, int relativityDestinationProviderCount = 1, int integrationPointTypesCount = 1)
        {
            _relativitySourceProviders.AddRange(Enumerable
                .Repeat(_RELATIVITY_SOURCE_PROVIDER_ID, relativitySourceProviderCount));
            _relativityDestinationProviders.AddRange(Enumerable
                .Repeat(_RELATIVITY_DESTINATION_PROVIDER_ID, relativityDestinationProviderCount));
            _integrationPointTypesList.AddRange(Enumerable
                .Repeat(_INTEGRATION_POINT_EXPORT_TYPE_ID, integrationPointTypesCount));
        }
    }
}
