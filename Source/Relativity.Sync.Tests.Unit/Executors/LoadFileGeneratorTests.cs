using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class LoadFileGeneratorTests
    {
        private const string _BATCH_GUID = "00000000-2222-4444-0000-888888888888";
        private const string _EXPORT_RUN_ID = "11111111-6666-2222-7777-333333333333";
        private const int _DESTINATION_WORKSPACE_ID = -1000001;
        private const int _SOURCE_WORKSPACE_ID = -1000000;
        private const int _CONFIGURATION_ID = -12678;

        private Mock<IBatchDataSourcePreparationConfiguration> _configurationMock;
        private Mock<IFileShareService> _fileshareServiceMock;
        private Mock<ISourceWorkspaceDataReaderFactory> _dataReaderFactoryMock;
        private Mock<ISourceWorkspaceDataReader> _dataReaderMock;
        private Mock<IItemStatusMonitor> _itemStatusMonitorMock;
        private Mock<IItemLevelErrorHandler> _itemLevelErrorHandlerMock;
        private Mock<IBatch> _batchMock;
        private Mock<IInstanceSettings> _instanceSettingsMock;
        private Mock<IAPILog> _loggerMock;
        private CompositeCancellationTokenStub _token;

        private string _serverPath;
        private string _workspacePath;

        private LoadFileGenerator _sut;

        [SetUp]
        public void SetUp()
        {
            _configurationMock = new Mock<IBatchDataSourcePreparationConfiguration>();
            _fileshareServiceMock = new Mock<IFileShareService>();
            _dataReaderFactoryMock = new Mock<ISourceWorkspaceDataReaderFactory>();
            _dataReaderMock = new Mock<ISourceWorkspaceDataReader>();
            _itemStatusMonitorMock = new Mock<IItemStatusMonitor>();
            _itemLevelErrorHandlerMock = new Mock<IItemLevelErrorHandler>();
            _batchMock = new Mock<IBatch>();
            _instanceSettingsMock = new Mock<IInstanceSettings>();
            _loggerMock = new Mock<IAPILog>();
            _token = new CompositeCancellationTokenStub();

            _configurationMock.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ID);
            _configurationMock.Setup(x => x.ExportRunId).Returns(It.IsAny<Guid>());
            _configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);
            _configurationMock.Setup(x => x.SyncConfigurationArtifactId).Returns(_CONFIGURATION_ID);

            _batchMock.Setup(x => x.BatchGuid).Returns(new Guid(_BATCH_GUID));
            _batchMock.Setup(x => x.ExportRunId).Returns(new Guid(_EXPORT_RUN_ID));

            _dataReaderFactoryMock.Setup(x => x.CreateNativeSourceWorkspaceDataReader(_batchMock.Object, _token.AnyReasonCancellationToken)).Returns(_dataReaderMock.Object);
            _dataReaderMock.Setup(x => x.ItemStatusMonitor).Returns(_itemStatusMonitorMock.Object);

            _instanceSettingsMock.Setup(x => x.GetImportAPIBatchStatusItemsUpdateCountAsync(It.IsAny<int>())).ReturnsAsync(1000);

            _sut = new LoadFileGenerator(
                _configurationMock.Object,
                _dataReaderFactoryMock.Object,
                _fileshareServiceMock.Object,
                _itemLevelErrorHandlerMock.Object,
                _instanceSettingsMock.Object,
                _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(_workspacePath) && Directory.Exists(_workspacePath))
            {
                var dir = new DirectoryInfo(_workspacePath);
                dir.Delete(recursive: true);
            }

            _serverPath = null;
            _workspacePath = null;
        }

        [Test]
        public async Task GenerateAsync_ShouldReturnILoadFileWithCorrectId()
        {
            // Arrange
            PrepareFakeLoadFilePath();

            _fileshareServiceMock.Setup(x => x.GetWorkspaceFileShareLocationAsync(_DESTINATION_WORKSPACE_ID))
                .ReturnsAsync(_workspacePath);

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock.Object, _token).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Id.ToString().Should().Be(_BATCH_GUID);
        }

        [Test]
        public async Task GenerateAsync_ShouldReturnCorrectFilePath_WhenRootDirectoryExists()
        {
            // Arrange
            string expectedBatchPath = PrepareFakeLoadFilePath();

            _fileshareServiceMock.Setup(x => x.GetWorkspaceFileShareLocationAsync(_DESTINATION_WORKSPACE_ID))
                .ReturnsAsync(_workspacePath);

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock.Object, _token).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Path.Should().Be(expectedBatchPath);
            result.Settings.Path.Should().Be(expectedBatchPath);
        }

        [Test]
        public void GenerateAsync_ShouldReturnError_WhenRootDirectoryDoesNotExist()
        {
            // Arrange
            _serverPath = "randomTestPath";
            string rootDirectory = $@"{_serverPath}\EDDS{_DESTINATION_WORKSPACE_ID}";
            string expectedErrorMessage = $"Unable to create load file path. Directory: {rootDirectory} does not exist!";

            _fileshareServiceMock.Setup(x => x.GetWorkspaceFileShareLocationAsync(_DESTINATION_WORKSPACE_ID))
                .ReturnsAsync(rootDirectory);

            // Act
            Func<Task<ILoadFile>> function = async () => await _sut.GenerateAsync(_batchMock.Object, _token).ConfigureAwait(false);

            // Assert
            function.Should().Throw<Exception>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public async Task GenerateAsync_ShouldHandleItemLevelErrors()
        {
            // Arrange
            int expectedNumberOfItemLevelErrors = 3;
            string testItemIdentifier = "testId";
            ItemLevelError testItemLevelError = new ItemLevelError(testItemIdentifier, "testMessage");
            long completedItemTestValue = 12345L;

            PrepareFakeLoadFilePath();

            _fileshareServiceMock.Setup(x => x.GetWorkspaceFileShareLocationAsync(_DESTINATION_WORKSPACE_ID))
                .ReturnsAsync(_workspacePath);

            _dataReaderMock.Setup(x => x.Read()).Callback(() =>
            {
                _dataReaderMock.Raise(x => x.OnItemReadError += null, completedItemTestValue, testItemLevelError);
                _dataReaderMock.Raise(x => x.OnItemReadError += null, completedItemTestValue, testItemLevelError);
                _dataReaderMock.Raise(x => x.OnItemReadError += null, completedItemTestValue, testItemLevelError);
            });

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock.Object, _token).ConfigureAwait(false);

            // Assert
            _itemLevelErrorHandlerMock.Verify(x => x.HandleItemLevelError(completedItemTestValue, testItemLevelError), Times.Exactly(expectedNumberOfItemLevelErrors));
            _itemLevelErrorHandlerMock.Verify(x => x.HandleRemainingErrorsAsync(), Times.Once);
        }

        [Test]
        public async Task GenerateAsync_ShouldStopProcessing_WhenCancellationTokenIsProvided()
        {
            // Arrange
            PrepareFakeLoadFilePath();

            _fileshareServiceMock.Setup(x => x.GetWorkspaceFileShareLocationAsync(_DESTINATION_WORKSPACE_ID))
                .ReturnsAsync(_workspacePath);
            bool readResult = true;
            _dataReaderMock.Setup(x => x.Read())
                .Returns(() => readResult).Callback(() => readResult = false);

            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsStopRequestedFunc = () => true
            };
            _dataReaderFactoryMock.Setup(x => x.CreateNativeSourceWorkspaceDataReader(_batchMock.Object, token.AnyReasonCancellationToken)).Returns(_dataReaderMock.Object);

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock.Object, token).ConfigureAwait(false);

            // Assert
            _batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Cancelled), Times.Once);
        }

        [Test]
        public async Task GenerateAsync_ShouldPreserveBatchStateAndStopProcessing_WhenDrainStopTokenIsProvided()
        {
            // Arrange
            PrepareFakeLoadFilePath();

            _fileshareServiceMock.Setup(x => x.GetWorkspaceFileShareLocationAsync(_DESTINATION_WORKSPACE_ID))
                .ReturnsAsync(_workspacePath);
            bool readResult = true;
            _dataReaderMock.Setup(x => x.Read())
                .Returns(() => readResult).Callback(() => readResult = false);

            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsDrainStopRequestedFunc = () => true
            };
            _dataReaderFactoryMock.Setup(x => x.CreateNativeSourceWorkspaceDataReader(_batchMock.Object, token.AnyReasonCancellationToken)).Returns(_dataReaderMock.Object);

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock.Object, token).ConfigureAwait(false);

            // Assert
            _batchMock.Verify(x => x.SetStatusAsync(BatchStatus.Paused), Times.Once);
            _batchMock.Verify(x => x.SetStartingIndexAsync(It.IsAny<int>()), Times.Once);
            _itemLevelErrorHandlerMock.Verify(x => x.HandleRemainingErrorsAsync(), Times.Once);
        }

        private string PrepareFakeLoadFilePath()
        {
            _serverPath = Path.GetTempPath();
            _workspacePath = Path.Combine(_serverPath, $@"EDDS{_DESTINATION_WORKSPACE_ID}");

            if (!Directory.Exists(_workspacePath))
            {
                Directory.CreateDirectory(_workspacePath);
            }

            return $@"{_workspacePath}\Sync\{_EXPORT_RUN_ID}\{_BATCH_GUID}\{_BATCH_GUID}.dat";
        }
    }
}
