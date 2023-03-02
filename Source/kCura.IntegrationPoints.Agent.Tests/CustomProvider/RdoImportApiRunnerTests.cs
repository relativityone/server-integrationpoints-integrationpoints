using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;

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
                .Setup(x => x.BuildAsync(It.IsAny<ImportSettings>(), It.IsAny<List<IndexedFieldMap>>()))
                .ReturnsAsync(_importConfiguration);

            _sut = new RdoImportApiRunner(
                _settingsBuilderMock.Object,
                _importApiServiceMock.Object,
                new Mock<IAPILog>().Object);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallSettingsBuilder()
        {
            // Arrange
            var configuration = _fxt.Create<ImportSettings>();
            var fieldMappings = _fxt.Create<List<IndexedFieldMap>>();

            // Act
            await _sut.RunImportJobAsync(_importJobContext, configuration, fieldMappings);

            // Assert
            _settingsBuilderMock.Verify(x => x.BuildAsync(configuration, fieldMappings), Times.Once);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallImportApiService()
        {
            // Arrange
            var configuration = _fxt.Create<ImportSettings>();
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
