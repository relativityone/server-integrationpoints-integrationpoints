using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1.Models.Sources;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Progress;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class BatchDataSourcePreparationExecutorTests
    {
        private const int _SOURCE_WORKSPACE_ID = 1000004;
        private const int _DESTINATION_WORKSPACE_ID = 1000005;
        private const int _CONFIGURATION_ID = 12345;
        private const string _EXPORT_RUN_ID = "11111111-2222-3333-4444-555555555555";

        private Mock<IBatchRepository> _batchRepositoryMock;
        private Mock<ILoadFileGenerator> _loadFileGeneratorMock;
        private Mock<IBatchDataSourcePreparationConfiguration> _configurationMock;
        private Mock<IProgressHandler> _progressHandlerMock;
        private Mock<IImportService> _importServiceMock;
        private Mock<IAPILog> _loggerMock;
        private BatchDataSourcePreparationExecutor _sut;

        [SetUp]
        public void SetUp()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _loadFileGeneratorMock = new Mock<ILoadFileGenerator>();
            _configurationMock = new Mock<IBatchDataSourcePreparationConfiguration>();
            _progressHandlerMock = new Mock<IProgressHandler>();
            _importServiceMock = new Mock<IImportService>();
            _loggerMock = new Mock<IAPILog>();

            SetupConfiguration();
            PrepareBatchList();

            _sut = new BatchDataSourcePreparationExecutor(
                _importServiceMock.Object,
                _batchRepositoryMock.Object,
                _loadFileGeneratorMock.Object,
                _progressHandlerMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_ShouldPrepareBatchDataSource_GoldFlow()
        {
            // Arrange
            Mock<IBatch> batchMock = new Mock<IBatch>();
            Mock<ILoadFile> loadFileMock = new Mock<ILoadFile>();
            _batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchMock.Object);
            _loadFileGeneratorMock.Setup(x => x.GenerateAsync(batchMock.Object, CompositeCancellationToken.None)).ReturnsAsync(loadFileMock.Object);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            _importServiceMock.Verify(x => x.AddDataSourceAsync(It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()), Times.Once);
            batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Generated), Times.Once);
            _importServiceMock.Verify(x => x.EndJobAsync(), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldEnd_WhenLoadFileGenerationFails()
        {
            // Arrange
            Exception exception = new Exception("generate load file exception");
            Mock<IBatch> batchMock = new Mock<IBatch>();
            Mock<ILoadFile> loadFileMock = new Mock<ILoadFile>();

            _batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchMock.Object);
            _loadFileGeneratorMock.Setup(x => x.GenerateAsync(batchMock.Object, CompositeCancellationToken.None)).Throws(exception);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _importServiceMock.Verify(x => x.AddDataSourceAsync(It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()), Times.Never);
            batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Generated), Times.Never);
            _importServiceMock.Verify(x => x.EndJobAsync(), Times.Never);
            _importServiceMock.Verify(x => x.CancelJobAsync(), Times.Once);
            _loggerMock.Verify(x => x.LogError(exception, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldEnd_WhenLoadFileIsNotSuccessfullyAddedToImportSource()
        {
            // Arrange
            Mock<IBatch> batchMock = new Mock<IBatch>();
            Mock<ILoadFile> loadFileMock = new Mock<ILoadFile>();

            _importServiceMock.Setup(x => x.AddDataSourceAsync(It.IsAny<Guid>(), It.IsAny<DataSourceSettings>())).Throws(new SyncException());
            _batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchMock.Object);
            _loadFileGeneratorMock.Setup(x => x.GenerateAsync(batchMock.Object, CompositeCancellationToken.None)).ReturnsAsync(loadFileMock.Object);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Generated), Times.Never);
            _importServiceMock.Verify(x => x.EndJobAsync(), Times.Never);
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCancelJob_WhenCancellationTokenIsProvided()
        {
            // Arrange
            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsStopRequestedFunc = () => true
            };

            Mock<IBatch> batchMock = new Mock<IBatch>();
            Mock<ILoadFile> loadFileMock = new Mock<ILoadFile>();

            _batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchMock.Object);
            _loadFileGeneratorMock.Setup(x => x.GenerateAsync(batchMock.Object, token)).ReturnsAsync(loadFileMock.Object);
            batchMock.Setup(x => x.Status).Returns(BatchStatus.Cancelled);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, token).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            _importServiceMock.Verify(x => x.CancelJobAsync(), Times.Once);
            _importServiceMock.Verify(x => x.AddDataSourceAsync(It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()), Times.Never);
            batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Generated), Times.Never);
            _importServiceMock.Verify(x => x.EndJobAsync(), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_ShouldPauseJob_WhenDrainStopTokenIsProvided()
        {
            // Arrange
            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsDrainStopRequestedFunc = () => true
            };

            Mock<IBatch> batchMock = new Mock<IBatch>();
            Mock<ILoadFile> loadFileMock = new Mock<ILoadFile>();

            _batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchMock.Object);
            _loadFileGeneratorMock.Setup(x => x.GenerateAsync(batchMock.Object, token)).ReturnsAsync(loadFileMock.Object);
            batchMock.Setup(x => x.Status).Returns(BatchStatus.Paused);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, token).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Paused);
            _importServiceMock.Verify(x => x.AddDataSourceAsync(It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()), Times.Never);
            batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Generated), Times.Never);
            _importServiceMock.Verify(x => x.EndJobAsync(), Times.Never);
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
