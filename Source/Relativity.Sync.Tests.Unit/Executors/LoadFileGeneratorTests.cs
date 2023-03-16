using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.Stubs;
using Relativity.Sync.Tests.Unit.Stubs;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ADLS;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class LoadFileGeneratorTests
    {
        private const int _DESTINATION_WORKSPACE_ID = -1000001;
        private const int _SOURCE_WORKSPACE_ID = -1000000;
        private const int _CONFIGURATION_ID = -12678;
        private readonly Guid _EXPORT_RUN_ID = Guid.NewGuid();
        private readonly Guid _BATCH_GUID = Guid.NewGuid();

        private BatchStub _batchMock;

        private Mock<IBatchDataSourcePreparationConfiguration> _configurationMock;
        private Mock<ILoadFilePathService> _loadFilePathServiceMock;
        private Mock<IStorageAccessService> _storageAccessFake;
        private Mock<ISourceWorkspaceDataReaderFactory> _dataReaderFactoryMock;
        private Mock<ISourceWorkspaceDataReader> _dataReaderMock;
        private Mock<IItemStatusMonitor> _itemStatusMonitorMock;
        private Mock<IItemLevelErrorHandler> _itemLevelErrorHandlerMock;
        private Mock<IInstanceSettings> _instanceSettingsMock;
        private Mock<Func<IStopwatch>> _stopwatchFactoryFake;
        private Mock<IStopwatch> _stopwatchFake;
        private Mock<ISyncMetrics> _syncMetricsMock;
        private Mock<IAPILog> _loggerMock;
        private CompositeCancellationTokenStub _token;

        private string _serverPath;
        private string _workspacePath;

        private LoadFileGenerator _sut;

        private string SyncJobPath => Path.Combine(_workspacePath, "Sync", _EXPORT_RUN_ID.ToString());

        private string BatchLoadFilePath => Path.Combine(SyncJobPath, _BATCH_GUID.ToString(), $"{_BATCH_GUID}.dat");

        [SetUp]
        public void SetUp()
        {
            PrepareBatchLoadFilePath();

            _configurationMock = new Mock<IBatchDataSourcePreparationConfiguration>();
            _loadFilePathServiceMock = new Mock<ILoadFilePathService>();
            _dataReaderFactoryMock = new Mock<ISourceWorkspaceDataReaderFactory>();
            _dataReaderMock = new Mock<ISourceWorkspaceDataReader>();
            _itemStatusMonitorMock = new Mock<IItemStatusMonitor>();
            _itemLevelErrorHandlerMock = new Mock<IItemLevelErrorHandler>();
            _batchMock = new BatchStub();
            _instanceSettingsMock = new Mock<IInstanceSettings>();
            _loggerMock = new Mock<IAPILog>();
            _token = new CompositeCancellationTokenStub();

            _configurationMock.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ID);
            _configurationMock.Setup(x => x.ExportRunId).Returns(It.IsAny<Guid>());
            _configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);
            _configurationMock.Setup(x => x.SyncConfigurationArtifactId).Returns(_CONFIGURATION_ID);

            _batchMock.BatchGuid = _BATCH_GUID;
            _batchMock.ExportRunId = _EXPORT_RUN_ID;

            _loadFilePathServiceMock.Setup(x => x.GenerateBatchLoadFilePathAsync(_batchMock)).ReturnsAsync(BatchLoadFilePath);

            _dataReaderFactoryMock.Setup(x => x.CreateNativeSourceWorkspaceDataReader(_batchMock, _token.AnyReasonCancellationToken)).Returns(_dataReaderMock.Object);
            _dataReaderMock.Setup(x => x.ItemStatusMonitor).Returns(_itemStatusMonitorMock.Object);

            _instanceSettingsMock.Setup(x => x.GetImportAPIBatchStatusItemsUpdateCountAsync(It.IsAny<int>())).ReturnsAsync(1000);

            _stopwatchFake = new Mock<IStopwatch>();

            _stopwatchFactoryFake = new Mock<Func<IStopwatch>>();
            _stopwatchFactoryFake.Setup(x => x()).Returns(_stopwatchFake.Object);

            _syncMetricsMock = new Mock<ISyncMetrics>();

            StorageAccessServiceMock storageAccessService = new StorageAccessServiceMock();

            _sut = new LoadFileGenerator(
                _configurationMock.Object,
                _dataReaderFactoryMock.Object,
                _itemLevelErrorHandlerMock.Object,
                _instanceSettingsMock.Object,
                _loadFilePathServiceMock.Object,
                _stopwatchFactoryFake.Object,
                _syncMetricsMock.Object,
                storageAccessService,
                _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(_workspacePath) && Directory.Exists(_workspacePath))
            {
                Directory.Delete(_workspacePath, true);
            }

            _serverPath = null;
            _workspacePath = null;
        }

        [Test]
        public async Task GenerateAsync_ShouldReturnILoadFileWithCorrectId()
        {
            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock, _token).ConfigureAwait(false);

            // Assert
            result.Id.Should().Be(_BATCH_GUID);
        }

        [Test]
        public async Task GenerateAsync_ShouldSendMetrics()
        {
            // Act
            await _sut.GenerateAsync(_batchMock, _token).ConfigureAwait(false);

            // Assert
            _syncMetricsMock.Verify(x => x.Send(It.IsAny<BatchLoadFileMetric>()), Times.Once);
        }

        [Test]
        public async Task GenerateAsync_ShouldFailBatch_WhenExceptionInLoadFileGeneration()
        {
            // Arrange
            _itemLevelErrorHandlerMock.Setup(x => x.HandleRemainingErrorsAsync())
                .Throws<ServiceException>();

            // Act
            Func<Task<ILoadFile>> function = async () => await _sut.GenerateAsync(_batchMock, _token).ConfigureAwait(false);

            // Assert
            function.Should().Throw<ServiceException>();

            _syncMetricsMock.Verify(x => x.Send(It.IsAny<BatchLoadFileMetric>()), Times.Once);
        }

        [Test]
        public async Task GenerateAsync_ShouldReturnCorrectFilePath_WhenRootDirectoryExists()
        {
            // Arrange
            string expectedBatchPath = BatchLoadFilePath;

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock, _token).ConfigureAwait(false);

            // Assert
            result.Path.Should().Be(expectedBatchPath);
            result.Settings.Path.Should().Be(expectedBatchPath);
        }

        [Test]
        public void GenerateAsync_ShouldReturnError_WhenRootDirectoryDoesNotExist()
        {
            // Arrange
            _loadFilePathServiceMock.Setup(x => x.GenerateBatchLoadFilePathAsync(_batchMock))
                .Throws<DirectoryNotFoundException>();

            // Act
            Func<Task<ILoadFile>> function = async () => await _sut.GenerateAsync(_batchMock, _token).ConfigureAwait(false);

            // Assert
            function.Should().Throw<DirectoryNotFoundException>();
        }

        [Test]
        public async Task GenerateAsync_ShouldHandleItemLevelErrors()
        {
            // Arrange
            int expectedNumberOfItemLevelErrors = 3;
            string testItemIdentifier = "testId";
            ItemLevelError testItemLevelError = new ItemLevelError(testItemIdentifier, "testMessage");
            long completedItemTestValue = 12345L;

            _dataReaderMock.Setup(x => x.Read()).Callback(() =>
            {
                _dataReaderMock.Raise(x => x.OnItemReadError += null, completedItemTestValue, testItemLevelError);
                _dataReaderMock.Raise(x => x.OnItemReadError += null, completedItemTestValue, testItemLevelError);
                _dataReaderMock.Raise(x => x.OnItemReadError += null, completedItemTestValue, testItemLevelError);
            });

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock, _token).ConfigureAwait(false);

            // Assert
            _itemLevelErrorHandlerMock.Verify(x => x.HandleItemLevelError(completedItemTestValue, testItemLevelError), Times.Exactly(expectedNumberOfItemLevelErrors));
            _itemLevelErrorHandlerMock.Verify(x => x.HandleRemainingErrorsAsync(), Times.Once);
        }

        [Test]
        public async Task GenerateAsync_ShouldStopProcessing_WhenCancellationTokenIsProvided()
        {
            // Arrange
            bool readResult = true;
            _dataReaderMock.Setup(x => x.Read())
                .Returns(() => readResult).Callback(() => readResult = false);

            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsStopRequestedFunc = () => true
            };
            _dataReaderFactoryMock.Setup(x => x.CreateNativeSourceWorkspaceDataReader(_batchMock, token.AnyReasonCancellationToken)).Returns(_dataReaderMock.Object);

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock, token).ConfigureAwait(false);

            // Assert
            _batchMock.Status.Should().Be(BatchStatus.Cancelled);
        }

        [Test]
        public async Task GenerateAsync_ShouldPreserveBatchStateAndStopProcessing_WhenDrainStopTokenIsProvided()
        {
            // Arrange
            const int expectedStartingIndex = 4;
            int index = 0;

            Func<bool> drainStopRequestFunc = () => index == expectedStartingIndex;

            Mock<IItemStatusMonitor> monitorMock = new Mock<IItemStatusMonitor>();
            monitorMock.SetupGet(x => x.ReadItemsCount).Returns(expectedStartingIndex);

            _dataReaderMock.Setup(x => x.Read())
                .Returns(() => index != expectedStartingIndex - 1).Callback(() => ++index);
            _dataReaderMock.SetupGet(x => x.ItemStatusMonitor).Returns(monitorMock.Object);

            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsDrainStopRequestedFunc = drainStopRequestFunc
            };
            _dataReaderFactoryMock.Setup(x => x.CreateNativeSourceWorkspaceDataReader(_batchMock, token.AnyReasonCancellationToken)).Returns(_dataReaderMock.Object);

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock, token).ConfigureAwait(false);

            // Assert
            _batchMock.Status.Should().Be(BatchStatus.Paused);
            _batchMock.StartingIndex.Should().Be(expectedStartingIndex);

            _itemLevelErrorHandlerMock.Verify(x => x.HandleRemainingErrorsAsync(), Times.Once);
        }

        private void PrepareBatchLoadFilePath()
        {
            _serverPath = Path.GetTempPath();
            _workspacePath = Path.Combine(_serverPath, $@"EDDS{_DESTINATION_WORKSPACE_ID}");

            if (!Directory.Exists(_workspacePath))
            {
                Directory.CreateDirectory(_workspacePath);
            }
        }
    }
}
