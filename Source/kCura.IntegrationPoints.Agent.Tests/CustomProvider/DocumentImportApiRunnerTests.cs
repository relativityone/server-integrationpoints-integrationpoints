using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    internal class DocumentImportApiRunnerTests
    {
        private IFixture _fxt;
        private ImportJobContext _importJobContext;
        private DocumentImportConfiguration _importConfiguration;

        private Mock<IDocumentImportSettingsBuilder> _settingsBuilderMock;
        private Mock<IImportApiService> _importApiServiceMock;
        private DocumentImportApiRunner _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();
            _importConfiguration = _fxt.Create<DocumentImportConfiguration>();
            _importJobContext = _fxt.Create<ImportJobContext>();

            _importApiServiceMock = new Mock<IImportApiService>();
            _settingsBuilderMock = new Mock<IDocumentImportSettingsBuilder>();
            _settingsBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<DestinationConfiguration>(), It.IsAny<List<IndexedFieldMap>>()))
                .ReturnsAsync(_importConfiguration);

            _sut = new DocumentImportApiRunner(
                _settingsBuilderMock.Object,
                _importApiServiceMock.Object,
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
            _settingsBuilderMock.Verify(x => x.BuildAsync(integrationPointInfo.DestinationConfiguration, integrationPointInfo.FieldMap), Times.Once);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallImportApiService()
        {
            // Arrange
            var integrationPointInfo = _fxt.Create<IntegrationPointInfo>();

            // Act
            await _sut.RunImportJobAsync(_importJobContext, integrationPointInfo);

            // Assert
            _importApiServiceMock.Verify(x => x.CreateImportJobAsync(_importJobContext), Times.Once);
            _importApiServiceMock.Verify(x => x.ConfigureDocumentImportApiJobAsync(_importJobContext, _importConfiguration), Times.Once);
            _importApiServiceMock.Verify(x => x.StartImportJobAsync(_importJobContext), Times.Once);
        }
    }
}
