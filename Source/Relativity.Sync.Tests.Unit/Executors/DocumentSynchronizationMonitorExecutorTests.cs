﻿using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Progress;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.Stubs;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal class DocumentSynchronizationMonitorExecutorTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<IProgressHandler> _progressHandlerMock;
        private Mock<IDocumentSynchronizationMonitorConfiguration> _configurationMock;
        private Mock<IItemLevelErrorHandler> _itemLevelErrorHandler;
        private Mock<IBatchRepository> _batchRepository;
        private Mock<IInstanceSettings> _instanceSettingsFake;
        private Mock<Sync.Executors.IImportService> _importServiceMock;

        private DocumentSynchronizationMonitorExecutor _sut;

        private IFixture _fxt;

        private int _DESTINATION_WORKSPACE_ID;
        private int _SOURCE_WORKSPACE_ID;
        private Guid _EXPORT_RUN_ID;

        [SetUp]
        public void Setup()
        {
            _fxt = FixtureFactory.Create();

            _DESTINATION_WORKSPACE_ID = _fxt.Create<int>();
            _SOURCE_WORKSPACE_ID = _fxt.Create<int>();
            _EXPORT_RUN_ID = _fxt.Create<Guid>();

            _loggerMock = new Mock<IAPILog>();
            _progressHandlerMock = new Mock<IProgressHandler>();
            _configurationMock = new Mock<IDocumentSynchronizationMonitorConfiguration>();
            _itemLevelErrorHandler = new Mock<IItemLevelErrorHandler>();
            _instanceSettingsFake = new Mock<IInstanceSettings>();
            _instanceSettingsFake.Setup(x => x.GetImportAPIStatusCheckDelayAsync(It.IsAny<TimeSpan>()))
                .ReturnsAsync(TimeSpan.FromMilliseconds(100));
            _importServiceMock = new Mock<Sync.Executors.IImportService>();

            _batchRepository = new Mock<IBatchRepository>();

            _configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);
            _configurationMock.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ID);
            _configurationMock.Setup(x => x.ExportRunId).Returns(_EXPORT_RUN_ID);

            _sut = new DocumentSynchronizationMonitorExecutor(
                _progressHandlerMock.Object,
                _itemLevelErrorHandler.Object,
                _batchRepository.Object,
                _instanceSettingsFake.Object,
                _importServiceMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_ShouldAlwaysExplicitlyUpdateProgressOnceTheJobIsFinished()
        {
            // Arrange
            PrepareTestDataSourceForBatch();

            PrepareImportDetailsConsecutiveResponse(ImportState.Completed);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _progressHandlerMock.Verify(x => x.HandleProgressAsync(), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnCompletedWithErrorsStatus_WhenAtLeastOneDataSourceHasItemLevelErrors()
        {
            // Arrange
            PrepareTestDataSourceForBatch(DataSourceState.CompletedWithItemErrors);

            PrepareImportDetailsConsecutiveResponse(ImportState.Completed);

            PrepareItemLevelErrorHandling();

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.CompletedWithErrors);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnFailedStatus_WhenMonitoringErrorOccurrs()
        {
            // Arrange
            PrepareTestDataSourceForBatch(DataSourceState.Inserting);

            _importServiceMock.Setup(x => x.GetJobImportProgressValueAsync()).Throws(new SyncException());

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Message.Should().NotBeEmpty();
        }

        [Test]
        public async Task ExecuteAsync_ShouldBePaused_OnDrainStop()
        {
            // Arrange
            PrepareTestDataSourceForBatch(DataSourceState.Inserting);
            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsDrainStopRequestedFunc = () => true
            };

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, token).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Paused);
            _importServiceMock.Verify(x => x.GetJobImportStatusAsync(), Times.Never);
            _importServiceMock.Verify(x => x.GetDataSourceProgressAsync(It.IsAny<Guid>()), Times.Never);
            _progressHandlerMock.Verify(x => x.HandleProgressAsync(), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallIAPICancellation_WhenCancelWasRequestedAtMonitoringStage()
        {
            // Arrange
            PrepareTestDataSourceForBatch();
            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsStopRequestedFunc = () => true
            };
            PrepareImportDetailsConsecutiveResponse(
                ImportState.Inserting, ImportState.Canceled, ImportState.Canceled);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, token).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Canceled);
            _importServiceMock.Verify(x => x.CancelJobAsync(), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldNotCallIAPICancellation_WhenCancelWasRequestedBeforeMonitoringStage()
        {
            // Arrange
            PrepareTestDataSourceForBatch();
            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsStopRequestedFunc = () => true
            };

            PrepareImportDetailsConsecutiveResponse(ImportState.Canceled, ImportState.Canceled);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, token).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Canceled);
            _importServiceMock.Verify(x => x.CancelJobAsync(), Times.Never);
        }

        [TestCase(ImportState.Canceled, ExecutionStatus.Canceled)]
        [TestCase(ImportState.Failed, ExecutionStatus.Failed)]
        [TestCase(ImportState.Completed, ExecutionStatus.Completed)]
        public async Task ExecuteAsync_ShouldReturnCorrectStatus_WhenImportStateIsKnown(ImportState jobFinalState, ExecutionStatus expectedStatus)
        {
            // Arrange
            PrepareTestDataSourceForBatch();

            PrepareImportDetailsConsecutiveResponse(
                ImportState.Scheduled,
                ImportState.Inserting,
                jobFinalState);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(expectedStatus);
        }

        [Test]
        public async Task ExecuteAsync_ShouldUpdateBatch_WhenDataSourceIsFinished()
        {
            // Arrange
            ImportProgress dataSourceProgress = _fxt.Create<ImportProgress>();

            IBatch batch = PrepareTestDataSourceForBatch(DataSourceState.CompletedWithItemErrors, dataSourceProgress);

            PrepareImportDetailsConsecutiveResponse(ImportState.Completed);

            PrepareItemLevelErrorHandling();

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.CompletedWithErrors);

            batch.TransferredDocumentsCount.Should().Be(dataSourceProgress.ImportedRecords);
            batch.FailedDocumentsCount.Should().Be(dataSourceProgress.ErroredRecords);
        }

        private IBatch PrepareTestDataSourceForBatch(
            DataSourceState dataSourceState = DataSourceState.Completed,
            ImportProgress importProgress = null)
        {
            BatchStub batch = _fxt.Build<BatchStub>()
                .With(x => x.BatchGuid, () => Guid.NewGuid())
                .With(x => x.Status, BatchStatus.Generated)
                .Create();

            _batchRepository.Setup(x => x.GetAllAsync(
                    _SOURCE_WORKSPACE_ID,
                    It.IsAny<int>(),
                    _EXPORT_RUN_ID))
                .ReturnsAsync(new[] { batch });

            DataSourceDetails dataSource = _fxt.Build<DataSourceDetails>()
                .With(x => x.State, dataSourceState)
                .Create();

            _importServiceMock.Setup(x => x.GetDataSourceStatusAsync(batch.BatchGuid))
                .ReturnsAsync(ValueResponse<DataSourceDetails>.CreateForSuccess(_EXPORT_RUN_ID, dataSource).Value);

            ImportProgress dataSourceProgress = importProgress ?? _fxt.Create<ImportProgress>();

            _importServiceMock.Setup(x => x.GetDataSourceProgressAsync(batch.BatchGuid))
                 .ReturnsAsync(ValueResponse<ImportProgress>.CreateForSuccess(_EXPORT_RUN_ID, dataSourceProgress).Value);

            return batch;
        }

        private void PrepareImportDetailsConsecutiveResponse(
            params ImportState[] states)
        {
            var setupSequence = _importServiceMock.SetupSequence(
                x => x.GetJobImportStatusAsync());

            foreach (var state in states)
            {
                ImportDetails importDetails = new ImportDetails(
                    state,
                    _fxt.Create<string>(),
                    _fxt.Create<int>(),
                    _fxt.Create<DateTime>(),
                    _fxt.Create<int>(),
                    _fxt.Create<DateTime>());

                setupSequence = setupSequence.ReturnsAsync(
                    ValueResponse<ImportDetails>.CreateForSuccess(
                        _EXPORT_RUN_ID, importDetails).Value);
            }
        }

        private void PrepareItemLevelErrorHandling()
        {
            const int readRecords = 1000;
            const int totalCount = readRecords;

            ImportErrors errors = _fxt.Build<ImportErrors>()
                .With(x => x.TotalCount, totalCount)
                .With(x => x.NumberOfRecords, readRecords)
                .Create();

            _importServiceMock.Setup(x => x.GetDataSourceErrorsAsync(It.IsAny<Guid>(), 0, 1000))
                .ReturnsAsync(ValueResponse<ImportErrors>.CreateForSuccess(_EXPORT_RUN_ID, errors).Value);
        }
    }
}
