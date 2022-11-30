using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
    [TestFixture]
    public class ImageJobEndMetricsServiceTests
    {
        private ImageJobEndMetricsService _sut;

        private Mock<IJobEndMetricsConfiguration> _jobEndMetricsConfigurationFake;
        private Mock<IBatchRepository> _batchRepositoryFake;
        private Mock<ISyncMetrics> _syncMetricsMock;
        private Mock<IJobStatisticsContainer> _jobStatisticsContainerFake;

        [SetUp]
        public void SetUp()
        {
            _batchRepositoryFake = new Mock<IBatchRepository>();

            _jobEndMetricsConfigurationFake = new Mock<IJobEndMetricsConfiguration>(MockBehavior.Loose);

            _syncMetricsMock = new Mock<ISyncMetrics>();

            _jobStatisticsContainerFake = new Mock<IJobStatisticsContainer>();

            _sut = new ImageJobEndMetricsService(_batchRepositoryFake.Object, _jobEndMetricsConfigurationFake.Object, _jobStatisticsContainerFake.Object, _syncMetricsMock.Object, new EmptyLogger());
        }

        [Test]
        public async Task ExecuteAsync_GoldFlowTest()
        {
            // Arrange
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

            const string expectedStatusDescription = "Completed with Errors";
            const int completedItemsPerBatch = 150;
            const int taggedItemsPerBatch = 150;
            const int failedItemsPerBatch = 1;
            int totalItemsCountPerBatch = completedItemsPerBatch + failedItemsPerBatch;

            var batch = new Mock<IBatch>();
            batch.SetupGet(x => x.TransferredDocumentsCount).Returns(completedItemsPerBatch);
            batch.SetupGet(x => x.FailedDocumentsCount).Returns(failedItemsPerBatch);
            batch.SetupGet(x => x.TaggedDocumentsCount).Returns(taggedItemsPerBatch);
            batch.SetupGet(x => x.TotalDocumentsCount).Returns(totalItemsCountPerBatch);
            var testBatches = new List<IBatch> { batch.Object, batch.Object };
            _batchRepositoryFake.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(testBatches);

            const ImportOverwriteMode overwriteMode = ImportOverwriteMode.AppendOnly;
            const DataSourceType sourceType = DataSourceType.SavedSearch;
            const DestinationLocationType destinationType = DestinationLocationType.Folder;
            const ImportImageFileCopyMode imageFileCopyMode = ImportImageFileCopyMode.CopyFiles;

            _jobEndMetricsConfigurationFake.SetupGet(x => x.ImportOverwriteMode).Returns(overwriteMode);
            _jobEndMetricsConfigurationFake.SetupGet(x => x.DataSourceType).Returns(sourceType);
            _jobEndMetricsConfigurationFake.SetupGet(x => x.DestinationType).Returns(destinationType);
            _jobEndMetricsConfigurationFake.SetupGet(x => x.ImportImageFileCopyMode).Returns(imageFileCopyMode);

            const long jobSize = 12345;
            const long imagesSize = 12345;
            _jobStatisticsContainerFake.SetupGet(x => x.FilesBytesTransferred).Returns(imagesSize);
            _jobStatisticsContainerFake.SetupGet(x => x.TotalBytesTransferred).Returns(jobSize);
            _jobStatisticsContainerFake.SetupGet(x => x.ImagesStatistics).Returns(Task.FromResult(new ImagesStatistics(2 * totalItemsCountPerBatch, imagesSize)));

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Status.Should().Be(ExecutionStatus.Completed);

            _syncMetricsMock.Verify(
                x => x.Send(It.Is<ImageJobEndMetric>(m =>
                m.TotalRecordsTransferred == completedItemsPerBatch * testBatches.Count &&
                m.TotalRecordsFailed == failedItemsPerBatch * testBatches.Count &&
                m.TotalRecordsRequested == totalItemsCountPerBatch * testBatches.Count &&
                m.TotalRecordsTagged == taggedItemsPerBatch * testBatches.Count &&
                m.JobEndStatus == expectedStatusDescription &&
                m.BytesImagesTransferred == imagesSize &&
                m.BytesTransferred == jobSize &&
                m.BytesImagesRequested == imagesSize &&
                m.OverwriteMode == overwriteMode &&
                m.SourceType == sourceType &&
                m.DestinationType == destinationType &&
                m.ImageFileCopyMode == imageFileCopyMode)), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldSendRetryMetric_WhenJobHasBeenRetried()
        {
            // Arrange
            const ExecutionStatus expectedStatus = ExecutionStatus.Completed;
            const string expectedStatusDescription = "Completed";

            _jobEndMetricsConfigurationFake.Setup(x => x.JobHistoryToRetryId).Returns(It.IsAny<int>());

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            _syncMetricsMock.Verify(x => x.Send(It.Is<ImageJobEndMetric>(m => m.RetryJobEndStatus == expectedStatusDescription)));
        }

        [Test]
        public async Task ExecuteAsync_ShouldBeCompleted_WhenGetBatchesThrowsException()
        {
            // Arrange
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

            _batchRepositoryFake.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).Throws<SyncException>();

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_ShouldNotReportImagesBytesTransferred_WhenNoFilesBytesTransferred()
        {
            // Arrange
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;
            _jobStatisticsContainerFake.SetupGet(x => x.FilesBytesTransferred).Returns(0);

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            _syncMetricsMock.Verify(x => x.Send(It.Is<ImageJobEndMetric>(m => m.BytesImagesTransferred == null)));
        }

        [Test]
        public async Task ExecuteAsync_ShouldNotReportTotalBytesTransferred_WhenNoTotalBytesTransferred()
        {
            // Arrange
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;
            _jobStatisticsContainerFake.SetupGet(x => x.TotalBytesTransferred).Returns(0);

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            _syncMetricsMock.Verify(x => x.Send(It.Is<ImageJobEndMetric>(m => m.BytesTransferred == null)));
        }

        [Test]
        public async Task ExecuteAsync_ShouldSendJobEndStatusMetric_WhenImagesStatisticsIsNull()
        {
            // Arrange
            const string expectedStatusDescription = "Completed with Errors";
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

            _jobStatisticsContainerFake.SetupGet(x => x.ImagesStatistics).Returns((Task<ImagesStatistics>)null);

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Status.Should().Be(ExecutionStatus.Completed);
            _syncMetricsMock.Verify(x => x.Send(It.Is<ImageJobEndMetric>(m => m.JobEndStatus == expectedStatusDescription)), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldSendJobEndStatusMetric_WhenImagesStatisticsThrowsException()
        {
            // Arrange
            const string expectedStatusDescription = "Completed with Errors";
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;
            Exception exception = new Exception();

            _jobStatisticsContainerFake.SetupGet(x => x.ImagesStatistics).Throws(exception);

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Status.Should().Be(ExecutionStatus.Completed);
            _syncMetricsMock.Verify(x => x.Send(It.Is<ImageJobEndMetric>(m => m.JobEndStatus == expectedStatusDescription)), Times.Once);
        }
    }
}
