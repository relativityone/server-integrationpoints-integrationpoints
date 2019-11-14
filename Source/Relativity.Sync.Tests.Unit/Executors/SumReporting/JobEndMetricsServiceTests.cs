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
		private JobEndMetricsService _instance;

		private Mock<IBatchRepository> _batchRepository;
		private Mock<IFieldManager> _fieldManager;
		private Mock<ISyncMetrics> _syncMetrics;
		private Mock<IJobStatisticsContainer> _jobStatisticsContainer;

		[SetUp]
		public void SetUp()
		{
			_batchRepository = new Mock<IBatchRepository>();
			Mock<IJobEndMetricsConfiguration> jobEndMetricsConfiguration = new Mock<IJobEndMetricsConfiguration>(MockBehavior.Loose);
			_fieldManager = new Mock<IFieldManager>();
			_syncMetrics = new Mock<ISyncMetrics>();
			_jobStatisticsContainer = new Mock<IJobStatisticsContainer>();

			_instance = new JobEndMetricsService(_batchRepository.Object, jobEndMetricsConfiguration.Object, _fieldManager.Object, _jobStatisticsContainer.Object, _syncMetrics.Object, new EmptyLogger());
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
			_batchRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(testBatches);

			const int testNumberOfFields = 10;
			IEnumerable<FieldInfoDto> fields = Enumerable.Repeat(FieldInfoDto.NativeFileFilenameField(), testNumberOfFields);
			_fieldManager.Setup(x => x.GetAllFieldsAsync(It.Is<CancellationToken>(c => c == CancellationToken.None))).ReturnsAsync(fields.ToList);

			const long jobSize = 12345;
			const long nativesSize = 5678;
			_jobStatisticsContainer.SetupGet(x => x.TotalBytesTransferred).Returns(jobSize);
			_jobStatisticsContainer.SetupGet(x => x.NativesBytesRequested).Returns(Task.FromResult(nativesSize));

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED, completedItemsPerBatch * testBatches.Count), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED, failedItemsPerBatch * testBatches.Count), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED, totalItemsCountPerBatch * testBatches.Count), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, expectedStatusDescription), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED, testNumberOfFields), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, jobSize), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED, nativesSize), Times.Once);
		}

		[Test]
		public async Task ItShouldNotLogBytesTransferredWhenNoNativeFilesHasBeenTransferred()
		{
			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(ExecutionStatus.Completed).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED, It.IsAny<long>()), Times.Never);
		}

		[Test]
		public async Task ItShouldNotReportBytesTransferredWhenJobFailed()
		{
			// Arrange
			const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;
			
			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED, It.IsAny<long>()), Times.Never);
		}

		[Test]
		public async Task ExecuteAsyncGetBatchesThrowsErrorAndLogsTest()
		{
			// Arrange
			const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

			_batchRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>())).Throws<SyncException>();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);
		}
	}
}