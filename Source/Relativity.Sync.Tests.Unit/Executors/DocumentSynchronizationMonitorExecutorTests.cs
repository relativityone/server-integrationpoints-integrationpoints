using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.Stubs;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class DocumentSynchronizationMonitorExecutorTests
    {
        private const int _DESTINATION_WORKSPACE_ID = -1000001;
        private const int _SOURCE_WORKSPACE_ID = -1000002;
        private const string _EXPORT_RUN_ID = "11111111-2222-3333-4444-555555555555";

        private Mock<IDestinationServiceFactoryForUser> _serviceFactoryMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IImportSourceController> _sourceControllerMock;
        private Mock<IProgressHandler> _progressHandlerMock;
        private Mock<IImportJobController> _jobControllerMock;
        private Mock<IDocumentSynchronizationMonitorConfiguration> _configurationMock;
        private Mock<IItemLevelErrorHandlerFactory> _itemLevelErrorHandlerFactory;
        private Mock<IImportApiItemLevelErrorHandler> _itemLevelErrorHandler;
        private Mock<IBatchRepository> _batchRepository;

        private DocumentSynchronizationMonitorExecutor _sut;

        [SetUp]
        public void Setup()
        {
            _serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
            _loggerMock = new Mock<IAPILog>();
            _progressHandlerMock = new Mock<IProgressHandler>();
            _sourceControllerMock = new Mock<IImportSourceController>();
            _jobControllerMock = new Mock<IImportJobController>();
            _configurationMock = new Mock<IDocumentSynchronizationMonitorConfiguration>();
            _itemLevelErrorHandlerFactory = new Mock<IItemLevelErrorHandlerFactory>();
            _itemLevelErrorHandler = new Mock<IImportApiItemLevelErrorHandler>();

            _itemLevelErrorHandlerFactory.Setup(x => x.CreateIApiHandler())
                .Returns(_itemLevelErrorHandler.Object);
            _batchRepository = new Mock<IBatchRepository>();

            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IImportSourceController>()).ReturnsAsync(_sourceControllerMock.Object);
            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IImportJobController>()).ReturnsAsync(_jobControllerMock.Object);

            _configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);
            _configurationMock.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ID);
            _configurationMock.Setup(x => x.ExportRunId).Returns(new Guid(_EXPORT_RUN_ID));

            _sut = new DocumentSynchronizationMonitorExecutor(_serviceFactoryMock.Object, _progressHandlerMock.Object, _itemLevelErrorHandlerFactory.Object, _batchRepository.Object, _loggerMock.Object);

            _sut.GetType()?.GetField("_delayTime", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(_sut, 0.1);
        }

        [Test]
        public async Task ExecuteAsync_ShouldAlwaysExplicitlyCallHandleProgressAsync()
        {
            // Arrange
            PrepareTestDataSources();

            ValueResponse<ImportDetails> importDetailsResponse = PrepareImportDetailsResponse(ImportState.Completed);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(importDetailsResponse);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _progressHandlerMock.Verify(x => x.HandleProgressAsync(), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallForJobStatus_UntilJobIsFinished()
        {
            // Arrange
            int expectedNumberOfJobStatusCalls = 3;
            PrepareTestDataSources();

            ValueResponse<ImportDetails> firstIterationDetailsResponse = PrepareImportDetailsResponse(ImportState.Scheduled);
            ValueResponse<ImportDetails> secondIterationDetailsResponse = PrepareImportDetailsResponse(ImportState.Inserting);
            ValueResponse<ImportDetails> thirdIterationDetailsResponse = PrepareImportDetailsResponse(ImportState.Completed);

            _jobControllerMock.SetupSequence(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(firstIterationDetailsResponse)
                .ReturnsAsync(secondIterationDetailsResponse)
                .ReturnsAsync(thirdIterationDetailsResponse);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            _jobControllerMock.Verify(
                x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId),
                Times.Exactly(expectedNumberOfJobStatusCalls));
        }

        [TestCase(ImportState.Canceled, "Canceled")]
        [TestCase(ImportState.Failed, "Failed")]
        [TestCase(ImportState.Completed, "Completed")]
        public async Task ExecuteAsync_ShouldReturnCorrectStatus_WhenImportStateIsKnown(ImportState jobFinalState, string expectedStatus)
        {
            // Arrange
            PrepareTestDataSources();

            ValueResponse<ImportDetails> jobDetailsResponse = PrepareImportDetailsResponse(jobFinalState);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(jobDetailsResponse);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.ToString().Should().Be(expectedStatus);
            _jobControllerMock.Verify(
                x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnCompletedWithErrorsStatus_WhenAtLeastOneDataSourceHasItemLevelErrors()
        {
            // Arrange
            PrepareTestDataSources(DataSourceState.CompletedWithItemErrors);

            ValueResponse<ImportDetails> jobDetailsResponse = PrepareImportDetailsResponse(ImportState.Completed);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(jobDetailsResponse);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.CompletedWithErrors);
            _jobControllerMock.Verify(
                x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnFailedStatus_WhenMonitoringErrorOccurrs()
        {
            // Arrange
            string testErrorMessage = "test error message";
            string testErrorCode = "ABC";
            PrepareTestDataSources(DataSourceState.Inserting);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(new ValueResponse<ImportDetails>(
                    It.IsAny<Guid>(),
                    false,
                    testErrorMessage,
                    testErrorCode,
                    null));
            string expectedErrorMessage = $"Job progress monitoring failed. Error code: {testErrorCode}, message: {testErrorMessage}";

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Message.Should().Be(expectedErrorMessage);
            _loggerMock.Verify(x => x.LogError(It.IsAny<SyncException>(), "Document synchronization monitoring error"));
        }

        [Test]
        public async Task ExecuteAsync_ShouldBePaused_OnDrainStop()
        {
            // Arrange
            PrepareTestDataSources(DataSourceState.Inserting);
            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsDrainStopRequestedFunc = () => true
            };

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, token).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Paused);
            _jobControllerMock.Verify(x => x.GetDetailsAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
            _sourceControllerMock.Verify(x => x.GetDetailsAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _progressHandlerMock.Verify(x => x.HandleProgressAsync(), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallIAPICancellation_WhenCancelWasRequestedAtMonitoringStage()
        {
            // Arrange
            PrepareTestDataSources(DataSourceState.Inserting);
            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsStopRequestedFunc = () => true
            };
            ValueResponse<ImportDetails> jobDetailsResponse = PrepareImportDetailsResponse(ImportState.Canceled);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(jobDetailsResponse);
            _jobControllerMock.Setup(x => x.CancelAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Response(It.IsAny<Guid>(), true, string.Empty, string.Empty));

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, token).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Canceled);
            _jobControllerMock.Verify(x => x.CancelAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
            _jobControllerMock.Verify(x => x.GetDetailsAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
            _progressHandlerMock.Verify(x => x.HandleProgressAsync(), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldNotCallIAPICancellation_WhenCancelWasRequestedBeforeMonitoringStage()
        {
            // Arrange
            PrepareTestDataSources(DataSourceState.Inserting, BatchStatus.Cancelled);
            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsStopRequestedFunc = () => true
            };
            ValueResponse<ImportDetails> jobDetailsResponse = PrepareImportDetailsResponse(ImportState.Canceled);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(jobDetailsResponse);
            _jobControllerMock.Setup(x => x.CancelAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Response(It.IsAny<Guid>(), true, string.Empty, string.Empty));

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, token).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Canceled);
            _jobControllerMock.Verify(x => x.CancelAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
            _jobControllerMock.Verify(x => x.GetDetailsAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
            _progressHandlerMock.Verify(x => x.HandleProgressAsync(), Times.Once);
        }

        private void PrepareTestDataSources(DataSourceState testedSourceState = DataSourceState.Completed, BatchStatus testedBatchInitialStatus = BatchStatus.New)
        {
            List<IBatch> testBatchList = new List<IBatch>();

            for (int i = 0; i < 3; i++)
            {
                BatchStub fakeBatch = new BatchStub
                {
                    BatchGuid = Guid.NewGuid(),
                    ExportRunId = new Guid(_EXPORT_RUN_ID),
                    Status = i == 0 ? testedBatchInitialStatus : BatchStatus.New
                };

                testBatchList.Add(fakeBatch);
            }

            _batchRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(testBatchList.ToArray());
            SetupDataSourceResultsSequence(testedSourceState);
        }

        private void SetupDataSourceResultsSequence(DataSourceState testedSourceState)
        {
            _sourceControllerMock.SetupSequence(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId, It.IsAny<Guid>()))
                .ReturnsAsync(new ValueResponse<DataSourceDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    new DataSourceDetails() { State = testedSourceState }))
                .ReturnsAsync(new ValueResponse<DataSourceDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    new DataSourceDetails() { State = DataSourceState.Completed }))
                .ReturnsAsync(new ValueResponse<DataSourceDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    new DataSourceDetails() { State = DataSourceState.Completed }));
        }

        private ValueResponse<ImportDetails> PrepareImportDetailsResponse(ImportState state)
        {
            return new ValueResponse<ImportDetails>(
                        It.IsAny<Guid>(),
                        true,
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        new ImportDetails(
                                state,
                                It.IsAny<string>(),
                                It.IsAny<int>(),
                                It.IsAny<DateTime>(),
                                It.IsAny<int>(),
                                It.IsAny<DateTime>()));
        }
    }
}
