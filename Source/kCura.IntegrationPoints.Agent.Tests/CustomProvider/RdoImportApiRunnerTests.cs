using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
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
    internal class RdoImportApiRunnerTests
    {
        private IFixture _fxt;
        private ImportJobContext _importJobContext;
        private RdoImportConfiguration _importConfiguration;

        private Mock<IRdoImportSettingsBuilder> _settingsBuilderMock;
        private Mock<IImportApiService> _importApiServiceMock;
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
                .Setup(x => x.Build(It.IsAny<DestinationConfiguration>(), It.IsAny<List<IndexedFieldMap>>()))
                .Returns(_importConfiguration);

            _sut = new RdoImportApiRunner(
                _settingsBuilderMock.Object,
                _importApiServiceMock.Object,
                new Mock<IAPILog>().Object);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallSettingsBuilder()
        {
            // Arrange
            var configuration = _fxt.Create<DestinationConfiguration>();
            var fieldMappings = _fxt.Create<List<IndexedFieldMap>>();

            // Act
            await _sut.RunImportJobAsync(_importJobContext, configuration, fieldMappings);

            // Assert
            _settingsBuilderMock.Verify(x => x.Build(configuration, fieldMappings), Times.Once);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallImportApiService()
        {
            // Arrange
            var configuration = _fxt.Create<DestinationConfiguration>();
            var fieldMappings = _fxt.Create<List<IndexedFieldMap>>();

            // Act
            await _sut.RunImportJobAsync(_importJobContext, configuration, fieldMappings);

            // Assert
            _importApiServiceMock.Verify(x => x.CreateImportJobAsync(_importJobContext), Times.Once);
            _importApiServiceMock.Verify(x => x.ConfigureRdoImportApiJobAsync(_importJobContext, _importConfiguration), Times.Once);
            _importApiServiceMock.Verify(x => x.StartImportJobAsync(_importJobContext), Times.Once);
        }
    }
}
