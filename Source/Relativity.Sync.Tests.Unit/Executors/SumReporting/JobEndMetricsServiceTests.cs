using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
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

		[SetUp]
		public void SetUp()
		{
			_batchRepository = new Mock<IBatchRepository>();
			Mock<IJobEndMetricsConfiguration> jobEndMetricsConfiguration = new Mock<IJobEndMetricsConfiguration>(MockBehavior.Loose);
			_fieldManager = new Mock<IFieldManager>();
			_syncMetrics = new Mock<ISyncMetrics>();
			Mock<ISyncLog> logger = new Mock<ISyncLog>();

			_instance = new JobEndMetricsService(_batchRepository.Object, jobEndMetricsConfiguration.Object, _fieldManager.Object, _syncMetrics.Object, logger.Object);
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
			IEnumerable<FieldInfoDto> fields = Enumerable.Repeat(FieldInfoDto.SourceWorkspaceField(), testNumberOfFields);
			_fieldManager.Setup(x => x.GetAllFieldsAsync(It.Is<CancellationToken>(c => c == CancellationToken.None))).ReturnsAsync(fields.ToList);

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED, completedItemsPerBatch * testBatches.Count, It.IsAny<string>()), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED, failedItemsPerBatch * testBatches.Count, It.IsAny<string>()), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED, totalItemsCountPerBatch * testBatches.Count, It.IsAny<string>()), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, expectedStatusDescription, It.IsAny<string>()), Times.Once);
			_syncMetrics.Verify(x => x.LogPointInTimeLong(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED, testNumberOfFields, It.IsAny<string>()), Times.Once);
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