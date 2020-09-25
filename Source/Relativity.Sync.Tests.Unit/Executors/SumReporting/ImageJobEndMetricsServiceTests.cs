using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Relativity.Sync.Logging;

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
			batch.SetupGet(x => x.TransferredItemsCount).Returns(completedItemsPerBatch);
			batch.SetupGet(x => x.TaggedItemsCount).Returns(taggedItemsPerBatch);
			batch.SetupGet(x => x.FailedItemsCount).Returns(failedItemsPerBatch);
			batch.SetupGet(x => x.TotalItemsCount).Returns(totalItemsCountPerBatch);
			var testBatches = new List<IBatch> { batch.Object, batch.Object };
			_batchRepositoryFake.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(testBatches);

			const long jobSize = 12345;
			const long imagesSize = 12345;
			_jobStatisticsContainerFake.SetupGet(x => x.FilesBytesTransferred).Returns(imagesSize);
			_jobStatisticsContainerFake.SetupGet(x => x.TotalBytesTransferred).Returns(jobSize);
			_jobStatisticsContainerFake.SetupGet(x => x.ImagesBytesRequested).Returns(Task.FromResult(imagesSize));

			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED, completedItemsPerBatch * testBatches.Count), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED, failedItemsPerBatch * testBatches.Count), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED, totalItemsCountPerBatch * testBatches.Count), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TAGGED, taggedItemsPerBatch * testBatches.Count), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, expectedStatusDescription), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_TRANSFERRED, imagesSize), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, jobSize), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_REQUESTED, imagesSize), Times.Once);
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
			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.RETRY_JOB_END_STATUS, expectedStatusDescription),
				Times.Once);
		}

		[Test]
		public async Task ExecuteAsync_ShouldBeCompleted_WhenGetBatchesThrowsException()
		{
			// Arrange
			const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

			_batchRepositoryFake.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>())).Throws<SyncException>();

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
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_TRANSFERRED, It.IsAny<long>()), Times.Never);
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
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, It.IsAny<long>()), Times.Never);
		}
	}
}
