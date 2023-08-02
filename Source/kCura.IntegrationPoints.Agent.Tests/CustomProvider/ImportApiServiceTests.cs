using System;
using System.Threading.Tasks;
using AutoFixture;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Common.Kepler;
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
    internal class ImportApiServiceTests
    {
        private const string RIP_APPLICATION_NAME = "rip";
        private IFixture _fxt;
        private ImportJobContext _importJobContext;
        private Response _successResponse;
        private Response _failedResponse;
        private DocumentImportConfiguration _documentConfiguration;
        private RdoImportConfiguration _rdoConfiguration;

        private Mock<IKeplerServiceFactory> _keplerServiceFactoryMock;
        private Mock<IImportJobController> _importJobControllerMock;
        private Mock<IDocumentConfigurationController> _documentConfigurationControllerMock;
        private Mock<IRDOConfigurationController> _rdoConfigurationControllerMock;
        private Mock<IAdvancedConfigurationController> _advancedConfigurationControllerMock;

        private ImportApiService _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();
            _importJobContext = _fxt.Create<ImportJobContext>();
            _successResponse = new Response(_fxt.Create<Guid>(), true, null, null);
            _failedResponse = new Response(_fxt.Create<Guid>(), false, _fxt.Create<string>(), _fxt.Create<string>());
            _documentConfiguration = _fxt.Create<DocumentImportConfiguration>();
            _rdoConfiguration = _fxt.Create<RdoImportConfiguration>();

            _importJobControllerMock = new Mock<IImportJobController>();
            _importJobControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, RIP_APPLICATION_NAME, _importJobContext.RipJobId.ToString()))
                .ReturnsAsync(_successResponse);
            _importJobControllerMock
                .Setup(x => x.BeginAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid))
                .ReturnsAsync(_successResponse);

            _documentConfigurationControllerMock = new Mock<IDocumentConfigurationController>();
            _documentConfigurationControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, _documentConfiguration.DocumentSettings))
                .ReturnsAsync(_successResponse);

            _rdoConfigurationControllerMock = new Mock<IRDOConfigurationController>();
            _rdoConfigurationControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, _rdoConfiguration.RdoSettings))
                .ReturnsAsync(_successResponse);

            _advancedConfigurationControllerMock = new Mock<IAdvancedConfigurationController>();
            _advancedConfigurationControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, It.IsAny<AdvancedImportSettings>()))
                .ReturnsAsync(_successResponse);

            _keplerServiceFactoryMock = new Mock<IKeplerServiceFactory>();
            _keplerServiceFactoryMock
                .Setup(x => x.CreateProxyAsync<IImportJobController>())
                .ReturnsAsync(_importJobControllerMock.Object);
            _keplerServiceFactoryMock
                .Setup(x => x.CreateProxyAsync<IDocumentConfigurationController>())
                .ReturnsAsync(_documentConfigurationControllerMock.Object);
            _keplerServiceFactoryMock
                .Setup(x => x.CreateProxyAsync<IRDOConfigurationController>())
                .ReturnsAsync(_rdoConfigurationControllerMock.Object);
            _keplerServiceFactoryMock
                .Setup(x => x.CreateProxyAsync<IAdvancedConfigurationController>())
                .ReturnsAsync(_advancedConfigurationControllerMock.Object);

            _sut = new ImportApiService(
                _keplerServiceFactoryMock.Object,
                new Mock<IAPILog>().Object);
        }

        [Test]
        public async Task CreateImportJobAsync_ShouldCallImportJobController()
        {
            // Act
            await _sut.CreateImportJobAsync(_importJobContext);

            // Assert
            _importJobControllerMock.Verify(
                x => x.CreateAsync(
                    _importJobContext.WorkspaceId,
                    _importJobContext.JobHistoryGuid,
                    RIP_APPLICATION_NAME,
                    _importJobContext.RipJobId.ToString()),
                Times.Once);
        }

        [Test]
        public async Task StartImportJobAsync_ShouldCallImportJobController()
        {
            // Act
            await _sut.StartImportJobAsync(_importJobContext);

            // Assert
            _importJobControllerMock.Verify(
                x => x.BeginAsync(
                    _importJobContext.WorkspaceId,
                    _importJobContext.JobHistoryGuid),
                Times.Once);
        }

        [Test]
        public async Task ConfigureDocumentImportApiJobAsync_ShouldCallConfigurationControllers()
        {
            // Act
            await _sut.ConfigureDocumentImportApiJobAsync(_importJobContext, _documentConfiguration);

            // Assert
            _documentConfigurationControllerMock.Verify(
                x => x.CreateAsync(
                    _importJobContext.WorkspaceId,
                    _importJobContext.JobHistoryGuid,
                    _documentConfiguration.DocumentSettings),
                Times.Once);

            _advancedConfigurationControllerMock.Verify(
                x => x.CreateAsync(
                    _importJobContext.WorkspaceId,
                    _importJobContext.JobHistoryGuid,
                    _documentConfiguration.AdvancedSettings),
                Times.Once);
        }

        [Test]
        public async Task ConfigureRdoImportApiJobAsync_ShouldCallConfigurationControllers()
        {
            // Act
            await _sut.ConfigureRdoImportApiJobAsync(_importJobContext, _rdoConfiguration);

            // Assert
            _rdoConfigurationControllerMock.Verify(
                x => x.CreateAsync(
                    _importJobContext.WorkspaceId,
                    _importJobContext.JobHistoryGuid,
                    _rdoConfiguration.RdoSettings),
                Times.Once);

            _advancedConfigurationControllerMock.Verify(
                x => x.CreateAsync(
                    _importJobContext.WorkspaceId,
                    _importJobContext.JobHistoryGuid,
                    _rdoConfiguration.AdvancedSettings),
                Times.Once);
        }

        [Test]
        public void CreateImportJobAsync_OnFailedCall_ShouldThrowException()
        {
            _importJobControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, RIP_APPLICATION_NAME, _importJobContext.RipJobId.ToString()))
                .ReturnsAsync(_failedResponse);

            // Act & Assert
            Assert.ThrowsAsync<ImportApiResponseException>(() => _sut.CreateImportJobAsync(_importJobContext));
        }

        [Test]
        public void StartImportJobAsync_OnFailedCall_ShouldThrowException()
        {
            _importJobControllerMock
                .Setup(x => x.BeginAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid))
                .ReturnsAsync(_failedResponse);

            // Act & Assert
            Assert.ThrowsAsync<ImportApiResponseException>(() => _sut.StartImportJobAsync(_importJobContext));
        }

        [Test]
        public void ConfigureDocumentImportApiJobAsync_OnFailedDocumentConfigureCall_ShouldThrowException()
        {
            _documentConfigurationControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, _documentConfiguration.DocumentSettings))
                .ReturnsAsync(_failedResponse);

            // Act & Assert
            Assert.ThrowsAsync<ImportApiResponseException>(() => _sut.ConfigureDocumentImportApiJobAsync(_importJobContext, _documentConfiguration));
        }

        [Test]
        public void ConfigureDocumentImportApiJobAsync_OnFailedAdvancedConfigureCall_ShouldThrowException()
        {
            _advancedConfigurationControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, _documentConfiguration.AdvancedSettings))
                .ReturnsAsync(_failedResponse);

            // Act & Assert
            Assert.ThrowsAsync<ImportApiResponseException>(() => _sut.ConfigureDocumentImportApiJobAsync(_importJobContext, _documentConfiguration));
        }

        [Test]
        public void ConfigureRdoImportApiJobAsync_OnFailedRdoConfigureCall_ShouldThrowException()
        {
            _rdoConfigurationControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, _rdoConfiguration.RdoSettings))
                .ReturnsAsync(_failedResponse);

            // Act & Assert
            Assert.ThrowsAsync<ImportApiResponseException>(() => _sut.ConfigureRdoImportApiJobAsync(_importJobContext, _rdoConfiguration));
        }

        [Test]
        public void ConfigureRdoImportApiJobAsync_OnFailedAdvancedConfigureCall_ShouldThrowException()
        {
            _advancedConfigurationControllerMock
                .Setup(x => x.CreateAsync(_importJobContext.WorkspaceId, _importJobContext.JobHistoryGuid, _rdoConfiguration.AdvancedSettings))
                .ReturnsAsync(_failedResponse);

            // Act & Assert
            Assert.ThrowsAsync<ImportApiResponseException>(() => _sut.ConfigureRdoImportApiJobAsync(_importJobContext, _rdoConfiguration));
        }
    }
}
