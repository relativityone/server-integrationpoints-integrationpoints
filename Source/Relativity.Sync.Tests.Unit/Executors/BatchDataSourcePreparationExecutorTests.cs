using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class BatchDataSourcePreparationExecutorTests
    {
        private const int _SOURCE_WORKSPACE_ID = 1000004;
        private const int _DESTINATION_WORKSPACE_ID = 1000005;
        private const int _CONFIGURATION_ID = 12345;
        private const string _EXPORT_RUN_ID = "11111111-2222-3333-4444-555555555555";

        private Mock<IDestinationServiceFactoryForUser> _serviceFactoryMock;
        private Mock<IBatchRepository> _batchRepositoryMock;
        private Mock<ILoadFileGenerator> _loadFileGeneratorMock;
        private Mock<IBatchDataSourcePreparationConfiguration> _configurationMock;
        private Mock<IImportSourceController> _importSourceControllerMock;
        private Mock<IImportJobController> _importJobControllerMock;
        private Mock<IAPILog> _loggerMock;
        private BatchDataSourcePreparationExecutor _sut;

        [SetUp]
        public void SetUp()
        {
            _serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _loadFileGeneratorMock = new Mock<ILoadFileGenerator>();
            _configurationMock = new Mock<IBatchDataSourcePreparationConfiguration>();
            _importSourceControllerMock = new Mock<IImportSourceController>();
            _importJobControllerMock = new Mock<IImportJobController>();
            _loggerMock = new Mock<IAPILog>();

            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IImportSourceController>()).ReturnsAsync(_importSourceControllerMock.Object);
            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IImportJobController>()).ReturnsAsync(_importJobControllerMock.Object);

            SetupConfiguration();
            PrepareBatchList();

            _sut = new BatchDataSourcePreparationExecutor(
                _serviceFactoryMock.Object,
                _batchRepositoryMock.Object,
                _loadFileGeneratorMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_ShouldPrepareBatchDataSource_GoldFlow()
        {
            // Arrange
            Mock<IBatch> batchMock = new Mock<IBatch>();
            Mock<ILoadFile> loadFileMock = new Mock<ILoadFile>();
            _batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchMock.Object);
            _loadFileGeneratorMock.Setup(x => x.GenerateAsync(batchMock.Object)).ReturnsAsync(loadFileMock.Object);
            _importSourceControllerMock.Setup(x => x.AddSourceAsync(
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DataSourceSettings>()))
                .ReturnsAsync(new Response(It.IsAny<Guid>(), true, string.Empty, string.Empty));

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            _importSourceControllerMock.Verify(x => x.AddSourceAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()), Times.Once);
            batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Generated), Times.Once);
            _importJobControllerMock.Verify(x => x.EndAsync(_DESTINATION_WORKSPACE_ID, It.IsAny<Guid>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldEndAsFailed_WhenLoadFileGenerationFails()
        {
            // Arrange
            Exception exception = new Exception("generate load file exception");
            Mock<IBatch> batchMock = new Mock<IBatch>();
            Mock<ILoadFile> loadFileMock = new Mock<ILoadFile>();
            _batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchMock.Object);
            _loadFileGeneratorMock.Setup(x => x.GenerateAsync(batchMock.Object)).Throws(exception);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _importSourceControllerMock.Verify(x => x.AddSourceAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()), Times.Never);
            batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Generated), Times.Never);
            _importJobControllerMock.Verify(x => x.EndAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Exception.Should().Be(exception);
            _loggerMock.Verify(x => x.LogError(exception, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldEndAsFailed_WhenLoadFileIsNotSuccessfullyAddedToImportSource()
        {
            // Arrange
            string expectedFailureMessage = "AddSourceAsync returned error";
            Mock<IBatch> batchMock = new Mock<IBatch>();
            Mock<ILoadFile> loadFileMock = new Mock<ILoadFile>();
            _batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchMock.Object);
            _loadFileGeneratorMock.Setup(x => x.GenerateAsync(batchMock.Object)).ReturnsAsync(loadFileMock.Object);

            _importSourceControllerMock.Setup(x => x.AddSourceAsync(
                _DESTINATION_WORKSPACE_ID,
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DataSourceSettings>()))
                .ReturnsAsync(new Response(It.IsAny<Guid>(), false, expectedFailureMessage, It.IsAny<string>()));

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Generated), Times.Never);
            _importJobControllerMock.Verify(x => x.EndAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Message.Should().Contain(expectedFailureMessage);
        }

        private void PrepareBatchList()
        {
            List<int> batchList = new List<int> { 123 };
            _batchRepositoryMock.Setup(x => x.GetAllBatchesIdsToExecuteAsync(
                _configurationMock.Object.SourceWorkspaceArtifactId,
                _configurationMock.Object.SyncConfigurationArtifactId,
                _configurationMock.Object.ExportRunId)).ReturnsAsync(batchList);
        }

        private void SetupConfiguration()
        {
            _configurationMock.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ID);
            _configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);
            _configurationMock.Setup(x => x.SyncConfigurationArtifactId).Returns(_CONFIGURATION_ID);
            _configurationMock.Setup(x => x.ExportRunId).Returns(new Guid(_EXPORT_RUN_ID));
        }
    }
}
