using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class ItemLevelErrorHandlerTests
    {
        private Mock<IItemLevelErrorHandlerConfiguration> _configurationMock;
        private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepositoryMock;
        private Mock<IItemStatusMonitor> _statusMonitorMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IBatch> _batchMock;

        private ItemLevelErrorHandler _sut;

        [SetUp]
        public void Setup()
        {
            PrepareFakeConfiguration();
            _jobHistoryErrorRepositoryMock = new Mock<IJobHistoryErrorRepository>();
            _statusMonitorMock = new Mock<IItemStatusMonitor>();
            _loggerMock = new Mock<IAPILog>();
            _batchMock = new Mock<IBatch>();
            _sut = new ItemLevelErrorHandler(_configurationMock.Object, _jobHistoryErrorRepositoryMock.Object, _statusMonitorMock.Object, _loggerMock.Object);
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
            _jobHistoryErrorRepositoryMock.Verify(x => x.MassCreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<CreateJobHistoryErrorDto>>()), Times.Never);
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
            _jobHistoryErrorRepositoryMock.Verify(
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
    }
}
