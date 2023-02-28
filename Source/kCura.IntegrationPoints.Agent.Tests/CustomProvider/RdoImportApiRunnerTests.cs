using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private const string RIP_APPLICATION_NAME = "Integration Points";
        private const int _workspaceId = 54321;
        private const long _ripJobId = 12345;

        private readonly Guid _importJobId = Guid.NewGuid();
        private readonly RdoImportConfiguration _importConfiguration = new RdoImportConfiguration(new ImportRdoSettings(), new AdvancedImportSettings());

        private Mock<IRdoImportSettingsBuilder> _settingsBuilderMock;
        private Mock<IKeplerServiceFactory> _keplerServiceFactoryMock;
        private Mock<IImportJobController> _importJobControllerMock;
        private Mock<IRDOConfigurationController> _rdoConfigurationControllerMock;
        private Mock<IAdvancedConfigurationController> _advancedConfigurationControllerMock;

        private RdoImportApiRunner _sut;

        [SetUp]
        public void SetUp()
        {
            var successResponse = new Response(Guid.NewGuid(), true, null, null);

            _settingsBuilderMock = new Mock<IRdoImportSettingsBuilder>();
            _settingsBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ImportSettings>(), It.IsAny<List<IndexedFieldMap>>()))
                .ReturnsAsync(_importConfiguration);

            _importJobControllerMock = new Mock<IImportJobController>();
            _importJobControllerMock
                .Setup(x => x.CreateAsync(_workspaceId, _importJobId, RIP_APPLICATION_NAME, _ripJobId.ToString()))
                .ReturnsAsync(successResponse);
            _importJobControllerMock
                .Setup(x => x.BeginAsync(_workspaceId, _importJobId))
                .ReturnsAsync(successResponse);

            _rdoConfigurationControllerMock = new Mock<IRDOConfigurationController>();
            _rdoConfigurationControllerMock
                .Setup(x => x.CreateAsync(_workspaceId, _importJobId, _importConfiguration.RdoSettings))
                .ReturnsAsync(successResponse);

            _advancedConfigurationControllerMock = new Mock<IAdvancedConfigurationController>();
            _advancedConfigurationControllerMock
                .Setup(x => x.CreateAsync(_workspaceId, _importJobId, _importConfiguration.AdvancedSettings))
                .ReturnsAsync(successResponse);

            _keplerServiceFactoryMock = new Mock<IKeplerServiceFactory>();
            _keplerServiceFactoryMock
                .Setup(x => x.CreateProxyAsync<IImportJobController>())
                .ReturnsAsync(_importJobControllerMock.Object);
            _keplerServiceFactoryMock
                .Setup(x => x.CreateProxyAsync<IRDOConfigurationController>())
                .ReturnsAsync(_rdoConfigurationControllerMock.Object);
            _keplerServiceFactoryMock
                .Setup(x => x.CreateProxyAsync<IAdvancedConfigurationController>())
                .ReturnsAsync(_advancedConfigurationControllerMock.Object);

            _sut = new RdoImportApiRunner(
                _settingsBuilderMock.Object,
                _keplerServiceFactoryMock.Object,
                new Mock<IAPILog>().Object);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallSettingsBuilder()
        {
            // Arrange
            var context = new ImportJobContext(_importJobId, _ripJobId, _workspaceId);
            var configuration = new ImportSettings();
            var fieldMappings = new List<IndexedFieldMap>();

            // Act
            await _sut.RunImportJobAsync(context, configuration, fieldMappings);

            // Assert
            _settingsBuilderMock.Verify(x => x.BuildAsync(configuration, fieldMappings), Times.Once);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallImportApiControllers()
        {
            // Arrange
            var context = new ImportJobContext(_importJobId, _ripJobId, _workspaceId);
            var configuration = new ImportSettings();
            var fieldMappings = new List<IndexedFieldMap>();

            // Act
            await _sut.RunImportJobAsync(context, configuration, fieldMappings);

            // Assert
            _importJobControllerMock.Verify(
                x => x.CreateAsync(
                        context.DestinationWorkspaceId,
                        context.ImportJobId,
                        RIP_APPLICATION_NAME,
                        context.RipJobId.ToString()),
                Times.Once);

            _importJobControllerMock.Verify(
                x => x.BeginAsync(
                    context.DestinationWorkspaceId,
                    context.ImportJobId),
                Times.Once);

            _rdoConfigurationControllerMock.Verify(
                x => x.CreateAsync(
                    context.DestinationWorkspaceId,
                    context.ImportJobId,
                    _importConfiguration.RdoSettings),
                Times.Once);

            _advancedConfigurationControllerMock.Verify(
                x => x.CreateAsync(
                    context.DestinationWorkspaceId,
                    context.ImportJobId,
                    _importConfiguration.AdvancedSettings),
                Times.Once);
        }

        [TestCase(false, false, false, false, false)]
        [TestCase(true, false, false, false, true)]
        [TestCase(false, true, false, false, true)]
        [TestCase(false, false, true, false, true)]
        [TestCase(false, false, false, true, true)]
        public async Task RunImportJobAsync_ShouldThrowOnFailure(
            bool createJobFailure,
            bool basicConfigFailure,
            bool advancedConfigFailure,
            bool beginJobFailure,
            bool throwException)
        {
            // Arrange
            var context = new ImportJobContext(_importJobId, _ripJobId, _workspaceId);
            var configuration = new ImportSettings();
            var fieldMappings = new List<IndexedFieldMap>();

            var failedResponse = new Response(Guid.NewGuid(), false, "error-message", "error-code");
            if (createJobFailure)
            {
                _importJobControllerMock
                    .Setup(x => x.CreateAsync(_workspaceId, _importJobId, RIP_APPLICATION_NAME, _ripJobId.ToString()))
                    .ReturnsAsync(failedResponse);
            }

            if (basicConfigFailure)
            {
                _rdoConfigurationControllerMock
                    .Setup(x => x.CreateAsync(_workspaceId, _importJobId, _importConfiguration.RdoSettings))
                    .ReturnsAsync(failedResponse);
            }

            if (advancedConfigFailure)
            {
                _advancedConfigurationControllerMock
                    .Setup(x => x.CreateAsync(_workspaceId, _importJobId, _importConfiguration.AdvancedSettings))
                    .ReturnsAsync(failedResponse);
            }


            if (beginJobFailure)
            {
                _importJobControllerMock
                    .Setup(x => x.BeginAsync(_workspaceId, _importJobId))
                    .ReturnsAsync(failedResponse);
            }

            // Act & Assert
            if (new[] { createJobFailure, basicConfigFailure, advancedConfigFailure, beginJobFailure }.Any(x => x))
            {
                Assert.ThrowsAsync<ImportApiResponseException>(() => _sut.RunImportJobAsync(context, configuration, fieldMappings));
            }
            else
            {
                await _sut.RunImportJobAsync(context, configuration, fieldMappings);
            }
        }
    }
}
