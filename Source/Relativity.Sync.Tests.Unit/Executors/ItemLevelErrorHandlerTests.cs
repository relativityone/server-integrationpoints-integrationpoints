using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Shared.V1.Exceptions;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common.Stubs;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class ItemLevelErrorHandlerTests
    {
        private readonly string _iApiBatchFilePath = Path.Combine(Path.GetTempPath(), "test.txt");

        private Mock<IItemLevelErrorHandlerConfiguration> _configurationMock;
        private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepositoryFake;
        private Mock<IItemStatusMonitor> _statusMonitorMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IBatch> _batchMock;

        private ItemLevelErrorHandler _sut;

        [SetUp]
        public void Setup()
        {
            PrepareFakeConfiguration();
            _jobHistoryErrorRepositoryFake = new Mock<IJobHistoryErrorRepository>();
            _statusMonitorMock = new Mock<IItemStatusMonitor>();
            _loggerMock = new Mock<IAPILog>();
            _batchMock = new Mock<IBatch>();
            _sut = new ItemLevelErrorHandler(_configurationMock.Object, _jobHistoryErrorRepositoryFake.Object, _statusMonitorMock.Object, _loggerMock.Object);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (File.Exists(_iApiBatchFilePath))
            {
                File.Delete(_iApiBatchFilePath);
            }
        }

        [Test]
        public void HandleItemLevelError_ShouldPrepareErrorEntry()
        {
            // Arrange
            ItemLevelError itemLevelError = new ItemLevelError("testId", "testMessage");

            // Act
            _sut.HandleItemLevelError(It.IsAny<long>(), itemLevelError);

            // Assert
            _statusMonitorMock.Verify(x => x.GetArtifactId(itemLevelError.Identifier), Times.Once);
            _statusMonitorMock.Verify(x => x.MarkItemAsFailed(itemLevelError.Identifier), Times.Once);
            _jobHistoryErrorRepositoryFake.Verify(x => x.MassCreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<CreateJobHistoryErrorDto>>()), Times.Never);
        }

        [Test]
        public async Task HandleDataSourceProcessingFinishedAsync_ShouldCreateCorrectNumberJobHistoryErrorEntries()
        {
            // Arrange
            int expectedItemLEvelErrorsCount = 3;
            PrepareTestItemLevelErrors(expectedItemLEvelErrorsCount);

            // Act
            await _sut.HandleDataSourceProcessingFinishedAsync(_batchMock.Object).ConfigureAwait(false);

            // Assert
            _jobHistoryErrorRepositoryFake.Verify(
                x => x.MassCreateAsync(
                _configurationMock.Object.SourceWorkspaceArtifactId,
                _configurationMock.Object.JobHistoryArtifactId,
                It.Is<List<CreateJobHistoryErrorDto>>(item => item.Count == expectedItemLEvelErrorsCount)), Times.Once);
        }

        [Test]
        public async Task HandleDataSourceProcessingFinishedAsync_ShouldCorrectlyUpdateBatchFailedDocumentsCount()
        {
            // Arrange
            int expectedItemLEvelErrorsCount = 10;
            _statusMonitorMock.Setup(x => x.FailedItemsCount).Returns(expectedItemLEvelErrorsCount);

            // Act
            await _sut.HandleDataSourceProcessingFinishedAsync(_batchMock.Object).ConfigureAwait(false);

            // Assert
            _batchMock.Verify(x => x.SetFailedDocumentsCountAsync(expectedItemLEvelErrorsCount));
        }

        [Test]
        public async Task HandleIApiItemLevelErrors_ShouldCreateItemLevelErrorsInJobHistory_WhenTheyExists()
        {
            // Arrange
            const int destinationWorkspaceArtifactId = 1001;
            Guid jobId = Guid.NewGuid();
            const int batchesCount = 3;
            const string identifier = "identifier";

            Mock<IDocumentSynchronizationMonitorConfiguration> documentSyncMonitorConfigurationMock = PrepareDocumentSyncMonitorConfigurationMock(destinationWorkspaceArtifactId, jobId);
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            List<IBatch> batches = PrepareBatches(batchesCount);

            PrepareGetItemErrorsAsync(importSourceControllerMock, identifier, jobId, destinationWorkspaceArtifactId, batches);
            PrepareGetDetailsAsync(importSourceControllerMock, jobId, destinationWorkspaceArtifactId, batches);

            // Act
            await _sut.HandleIApiItemLevelErrors(importSourceControllerMock.Object, batches, documentSyncMonitorConfigurationMock.Object);

            // Assert
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Exactly(batchesCount));
        }

        [Test]
        public async Task HandleIApiItemLevelErrors_ShouldNotCreateItemLevelErrorsInJobHistory_WhenTheyDontExists()
        {
            // Arrange
            const int destinationWorkspaceArtifactId = 1001;
            Guid jobId = Guid.NewGuid();
            const int batchesCount = 0;
            const string identifier = "identifier";

            Mock<IDocumentSynchronizationMonitorConfiguration> documentSyncMonitorConfigurationMock = PrepareDocumentSyncMonitorConfigurationMock(destinationWorkspaceArtifactId, jobId);
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            List<IBatch> batches = PrepareBatches(batchesCount);

            PrepareGetItemErrorsAsync(importSourceControllerMock, identifier, jobId, destinationWorkspaceArtifactId, batches);
            PrepareGetDetailsAsync(importSourceControllerMock, jobId, destinationWorkspaceArtifactId, batches);

            // Act
            await _sut.HandleIApiItemLevelErrors(importSourceControllerMock.Object, batches, documentSyncMonitorConfigurationMock.Object);

            // Assert
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Never);
        }

        [Test]
        public void HandleIApiItemLevelErrors_ShouldThrowException_WhenGetDataSourceDetailsAsyncCallFails()
        {
            // Arrange
            const int destinationWorkspaceArtifactId = 1001;
            Guid jobId = Guid.NewGuid();
            const int batchesCount = 3;
            const string identifier = "identifier";

            Mock<IDocumentSynchronizationMonitorConfiguration> documentSyncMonitorConfigurationMock = PrepareDocumentSyncMonitorConfigurationMock(destinationWorkspaceArtifactId, jobId);
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            List<IBatch> batches = PrepareBatches(batchesCount);

            PrepareGetItemErrorsAsync(importSourceControllerMock, identifier, jobId, destinationWorkspaceArtifactId, batches);
            PrepareGetDetailsAsync(importSourceControllerMock, jobId, destinationWorkspaceArtifactId, batches, false);

            // Act
            Func<Task> function = async () => await _sut.HandleIApiItemLevelErrors(importSourceControllerMock.Object, batches, documentSyncMonitorConfigurationMock.Object);

            // Assert
            function.Should().Throw<NotFoundException>();
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Never);
        }

        [Test]
        public void HandleIApiItemLevelErrors_ShouldThrowException_WhenGetItemErrorsAsyncCallFails()
        {
            // Arrange
            const int destinationWorkspaceArtifactId = 1001;
            Guid jobId = Guid.NewGuid();
            const int batchesCount = 3;
            const string identifier = "identifier";

            Mock<IDocumentSynchronizationMonitorConfiguration> documentSyncMonitorConfigurationMock = PrepareDocumentSyncMonitorConfigurationMock(destinationWorkspaceArtifactId, jobId);
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            List<IBatch> batches = PrepareBatches(batchesCount);

            PrepareGetItemErrorsAsync(importSourceControllerMock, identifier, jobId, destinationWorkspaceArtifactId, batches, false);
            PrepareGetDetailsAsync(importSourceControllerMock, jobId, destinationWorkspaceArtifactId, batches);

            // Act
            Func<Task> function = async () => await _sut.HandleIApiItemLevelErrors(importSourceControllerMock.Object, batches, documentSyncMonitorConfigurationMock.Object);

            // Assert
            function.Should().Throw<NotFoundException>();
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Never);
        }

        [Test]
        public async Task HandleIApiItemLevelErrors_ShouldCreateItemLevelErrorsInJobHistory_WhenThereIsNoIdentifierInResponse()
        {
            // Arrange
            const int destinationWorkspaceArtifactId = 1001;
            Guid jobId = Guid.NewGuid();
            const int batchesCount = 3;
            const string identifier = "identifier";

            Mock<IDocumentSynchronizationMonitorConfiguration> documentSyncMonitorConfigurationMock = PrepareDocumentSyncMonitorConfigurationMock(destinationWorkspaceArtifactId, jobId);
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            List<IBatch> batches = PrepareBatches(batchesCount);

            PrepareGetItemErrorsAsync(importSourceControllerMock, identifier, jobId, destinationWorkspaceArtifactId, batches, identifierName: string.Empty);
            PrepareGetDetailsAsync(importSourceControllerMock, jobId, destinationWorkspaceArtifactId, batches);
            File.WriteAllText(_iApiBatchFilePath, $"{identifier},1,2,3,4");

            // Act
            await _sut.HandleIApiItemLevelErrors(importSourceControllerMock.Object, batches, documentSyncMonitorConfigurationMock.Object);

            // Assert
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Exactly(batchesCount));
        }

        private void PrepareTestItemLevelErrors(int expectedItemLEvelErrorsCount)
        {
            ItemLevelError itemLevelError = new ItemLevelError("testId", "testMessage");
            for (int i = 0; i < expectedItemLEvelErrorsCount; i++)
            {
                _sut.HandleItemLevelError(It.IsAny<long>(), itemLevelError);
            }
        }

        private void PrepareFakeConfiguration()
        {
            _configurationMock = new Mock<IItemLevelErrorHandlerConfiguration>();
            _configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(It.IsAny<int>());
            _configurationMock.Setup(x => x.JobHistoryArtifactId).Returns(It.IsAny<int>());
        }

        private void PrepareGetDetailsAsync(
            Mock<IImportSourceController> importSourceControllerMock,
            Guid jobId,
            int destinationWorkspaceArtifactId,
            List<IBatch> batches,
            bool isSuccessful = true)
        {
            ValueResponse<DataSourceDetails> dataSourceDetailsResponse = new ValueResponse<DataSourceDetails>(
                jobId,
                isSuccessful,
                string.Empty,
                string.Empty,
                new DataSourceDetails
                {
                    DataSourceSettings = new DataSourceSettings()
                    {
                        ColumnDelimiter = ',',
                        Path = _iApiBatchFilePath
                    }
                });

            importSourceControllerMock.Setup(x => x.GetDetailsAsync(
                    destinationWorkspaceArtifactId,
                    jobId,
                    It.Is<Guid>(y => batches.Select(z => z.BatchGuid).Contains(y))))
                .ReturnsAsync(dataSourceDetailsResponse);
        }

        private void PrepareGetItemErrorsAsync(
            Mock<IImportSourceController> importSourceControllerMock,
            string identifier,
            Guid jobId,
            int destinationWorkspaceArtifactId,
            List<IBatch> batches,
            bool isSuccessful = true,
            string identifierName = "Identifier")
        {
            const int itemLevelErrorsPerBatch = 1000;
            List<ImportError> itemLevelErrorsList = new List<ImportError>();
            for (int i = 0; i < itemLevelErrorsPerBatch; i++)
            {
                ImportError importError = new ImportError(0, new List<ErrorDetail>
                {
                    new ErrorDetail(
                        0,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        new Dictionary<string, string>
                        {
                            { identifierName, identifier }
                        })
                });
                itemLevelErrorsList.Add(importError);
            }

            ValueResponse<ImportErrors> itemLevelErrorsResponse = new ValueResponse<ImportErrors>(
                jobId,
                isSuccessful,
                string.Empty,
                string.Empty,
                new ImportErrors(
                    Guid.Empty,
                    itemLevelErrorsList,
                    itemLevelErrorsList.Count,
                    0,
                    itemLevelErrorsList.Count));

            importSourceControllerMock.Setup(x =>
                    x.GetItemErrorsAsync(
                        destinationWorkspaceArtifactId,
                        jobId,
                        It.Is<Guid>(y => batches.Select(z => z.BatchGuid).Contains(y)),
                        0,
                        int.MaxValue))
                .ReturnsAsync(itemLevelErrorsResponse);
        }

        private static List<IBatch> PrepareBatches(int batchesCount)
        {
            List<IBatch> batches = new List<IBatch>();
            for (int i = 0; i < batchesCount; i++)
            {
                batches.Add(new BatchStub
                {
                    BatchGuid = Guid.NewGuid()
                });
            }

            return batches;
        }

        private static Mock<IDocumentSynchronizationMonitorConfiguration> PrepareDocumentSyncMonitorConfigurationMock(int destinationWorkspaceArtifactId, Guid jobId)
        {
            Mock<IDocumentSynchronizationMonitorConfiguration> documentSyncMonitorConfigurationMock =
                new Mock<IDocumentSynchronizationMonitorConfiguration>();

            documentSyncMonitorConfigurationMock
                .Setup(x => x.DestinationWorkspaceArtifactId)
                .Returns(destinationWorkspaceArtifactId);
            documentSyncMonitorConfigurationMock
                .Setup(x => x.ExportRunId)
                .Returns(jobId);
            return documentSyncMonitorConfigurationMock;
        }
    }
}
