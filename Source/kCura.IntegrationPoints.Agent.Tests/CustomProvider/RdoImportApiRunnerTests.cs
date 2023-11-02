using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    internal class RdoImportApiRunnerTests
    {
        private IFixture _fxt;
        private ImportJobContext _importJobContext;
        private RdoImportConfiguration _importConfiguration;

        private Mock<IRdoImportSettingsBuilder> _settingsBuilderMock;
        private Mock<IImportApiService> _importApiServiceMock;
        private Mock<IEntityFullNameService> _entityFullNameServiceMock;
        private RdoImportApiRunner _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();
            _importConfiguration = _fxt.Create<RdoImportConfiguration>();
            _importJobContext = _fxt.Create<ImportJobContext>();

            _importApiServiceMock = new Mock<IImportApiService>();
            _settingsBuilderMock = new Mock<IRdoImportSettingsBuilder>();
            _settingsBuilderMock
                .Setup(x => x.Build(It.IsAny<CustomProviderDestinationConfiguration>(), It.IsAny<List<IndexedFieldMap>>(), It.IsAny<IndexedFieldMap>()))
                .Returns(_importConfiguration);

            _entityFullNameServiceMock = new Mock<IEntityFullNameService>();
            _entityFullNameServiceMock.Setup(x => x.ShouldHandleFullNameAsync(
                    It.IsAny<CustomProviderDestinationConfiguration>(),
                    It.IsAny<List<IndexedFieldMap>>()))
                .ReturnsAsync(false);

            _sut = new RdoImportApiRunner(
                _settingsBuilderMock.Object,
                _importApiServiceMock.Object,
                _entityFullNameServiceMock.Object,
                new Mock<IAPILog>().Object);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallSettingsBuilder()
        {
            // Arrange
            var integrationPointInfo = _fxt.Create<IntegrationPointInfo>();

            // Act
            await _sut.RunImportJobAsync(_importJobContext, integrationPointInfo);

            // Assert
            _settingsBuilderMock.Verify(
                x => x.Build(
                    integrationPointInfo.DestinationConfiguration,
                    integrationPointInfo.FieldMap,
                    It.IsAny<IndexedFieldMap>()),
                Times.Once);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallImportApiService()
        {
            // Arrange
            var integrationPointInfo = _fxt.Create<IntegrationPointInfo>();
            var identifier = _fxt.Create<IndexedFieldMap>();

            // Act
            await _sut.RunImportJobAsync(_importJobContext, integrationPointInfo);

            // Assert
            _importApiServiceMock.Verify(x => x.CreateImportJobAsync(_importJobContext), Times.Once);
            _importApiServiceMock.Verify(x => x.ConfigureRdoImportApiJobAsync(_importJobContext, _importConfiguration), Times.Once);
            _importApiServiceMock.Verify(x => x.StartImportJobAsync(_importJobContext), Times.Once);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldBuildConfigurationWithCustomFieldAsOverlay_WhenFullNameIsNotAddedOnTheFly()
        {
            // Arrange
            var integrationPointInfo = _fxt.Create<IntegrationPointInfo>();
            var overlayIdentifier = integrationPointInfo.FieldMap.Last();

            integrationPointInfo.DestinationConfiguration.OverlayIdentifier = overlayIdentifier.DestinationFieldName;

            // Act
            await _sut.RunImportJobAsync(_importJobContext, integrationPointInfo);

            // Assert
            _settingsBuilderMock.Verify(
                x => x.Build(
                    integrationPointInfo.DestinationConfiguration,
                    integrationPointInfo.FieldMap,
                    overlayIdentifier),
                Times.Once);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldBuildConfigurationWithFullNameAsOverlay_WhenFullNameIsAddedOnTheFly()
        {
            // Arrange
            var integrationPointInfo = _fxt.Create<IntegrationPointInfo>();

            _entityFullNameServiceMock.Setup(x => x.ShouldHandleFullNameAsync(
                    It.IsAny<CustomProviderDestinationConfiguration>(),
                    It.IsAny<List<IndexedFieldMap>>()))
                .ReturnsAsync(true);
            _entityFullNameServiceMock
                .Setup(x => x.EnrichFieldMapWithFullNameAsync(It.IsAny<List<IndexedFieldMap>>(), It.IsAny<int>()))
                .Returns((List<IndexedFieldMap> fieldMap, int destinationWorkspaceArtifactId) =>
                {
                    FieldMap fieldEntry = _fxt.Create<FieldMap>();

                    fieldEntry.DestinationField.FieldIdentifier = EntityFieldNames.FullName;
                    fieldEntry.DestinationField.DisplayName = EntityFieldNames.FullName;

                    IndexedFieldMap fullNameField = new IndexedFieldMap(
                        fieldEntry, FieldMapType.Normal, 0);

                    fieldMap.Add(fullNameField);

                    return Task.CompletedTask;
                });

            var overlayIdentifier = integrationPointInfo.FieldMap.Last();

            integrationPointInfo.DestinationConfiguration.OverlayIdentifier = overlayIdentifier.DestinationFieldName;

            // Act
            await _sut.RunImportJobAsync(_importJobContext, integrationPointInfo);

            // Assert
            _settingsBuilderMock.Verify(
                x => x.Build(
                    integrationPointInfo.DestinationConfiguration,
                    integrationPointInfo.FieldMap,
                    It.Is<IndexedFieldMap>(y => y.DestinationFieldName == EntityFieldNames.FullName)),
                Times.Once);
        }
    }
}
