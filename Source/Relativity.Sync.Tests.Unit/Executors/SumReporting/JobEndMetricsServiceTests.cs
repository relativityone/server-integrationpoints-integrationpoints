using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	public class JobEndMetricsServiceTests
	{
		private JobEndMetricsService _sut;

		private Mock<IJobEndMetricsConfiguration> _jobEndMetricsConfigurationFake;
		private Mock<IBatchRepository> _batchRepositoryFake;
		private Mock<IFieldManager> _fieldManagerFake;
		private Mock<ISyncMetrics> _syncMetricsMock;
		private Mock<IJobStatisticsContainer> _jobStatisticsContainerFake;

		[SetUp]
		public void SetUp()
		{
			_batchRepositoryFake = new Mock<IBatchRepository>();
			_jobEndMetricsConfigurationFake = new Mock<IJobEndMetricsConfiguration>(MockBehavior.Loose);
			_fieldManagerFake = new Mock<IFieldManager>();
			_syncMetricsMock = new Mock<ISyncMetrics>();
			_jobStatisticsContainerFake = new Mock<IJobStatisticsContainer>();

			_sut = new JobEndMetricsService(_batchRepositoryFake.Object, _jobEndMetricsConfigurationFake.Object, _fieldManagerFake.Object, _jobStatisticsContainerFake.Object, _syncMetricsMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task ExecuteAsyncGoldFlowTest()
		{
			// Arrange
			const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

			const string expectedStatusDescription = "Completed with Errors";
			const int completedItemsPerBatch = 150;
			const int failedItemsPerBatch = 1;
			int totalItemsCountPerBatch = completedItemsPerBatch + failedItemsPerBatch;

			var batch = new Mock<IBatch>();
			batch.SetupGet(x => x.TransferredItemsCount).Returns(completedItemsPerBatch);
			batch.SetupGet(x => x.FailedItemsCount).Returns(failedItemsPerBatch);
			batch.SetupGet(x => x.TotalItemsCount).Returns(totalItemsCountPerBatch);
			var testBatches = new List<IBatch> { batch.Object, batch.Object };
			_batchRepositoryFake.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(testBatches);

			const int testNumberOfFields = 10;
			IEnumerable<FieldInfoDto> fields = Enumerable.Repeat(FieldInfoDto.NativeFileFilenameField(), testNumberOfFields);
			_fieldManagerFake.Setup(x => x.GetAllFieldsAsync(It.Is<CancellationToken>(c => c == CancellationToken.None))).ReturnsAsync(fields.ToList);

			const long jobSize = 12345;
			const long nativesSize = 5678;
			_jobStatisticsContainerFake.SetupGet(x => x.TotalBytesTransferred).Returns(jobSize);
			_jobStatisticsContainerFake.SetupGet(x => x.NativesBytesRequested).Returns(Task.FromResult(nativesSize));

			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED, completedItemsPerBatch * testBatches.Count), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED, failedItemsPerBatch * testBatches.Count), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED, totalItemsCountPerBatch * testBatches.Count), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, expectedStatusDescription), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED, testNumberOfFields), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, jobSize), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED, nativesSize), Times.Once);
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
		public async Task ItShouldNotLogBytesTransferredWhenNoNativeFilesHasBeenTransferred()
		{
			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(ExecutionStatus.Completed).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED, It.IsAny<long>()), Times.Never);
		}

		[Test]
		public async Task ItShouldNotReportBytesTransferredWhenJobFailed()
		{
			// Arrange
			const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;
			
			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, It.IsAny<long>()), Times.Never);
		}

		[Test]
		public async Task ExecuteAsyncGetBatchesThrowsErrorAndLogsTest()
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
	}
}