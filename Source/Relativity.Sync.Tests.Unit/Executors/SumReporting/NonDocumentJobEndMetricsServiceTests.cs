using System;
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
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	public class NonDocumentJobEndMetricsServiceTests
	{
		private NonDocumentJobEndMetricsService _sut;

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
			_fieldManagerFake.Setup(x => x.GetMappedFieldsNonDocumentWithoutLinksAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<FieldInfoDto>().AsReadOnly());
            _fieldManagerFake.Setup(x => x.GetAllAvailableFieldsToMap())
                .Returns(new List<FieldInfoDto>());

			_syncMetricsMock = new Mock<ISyncMetrics>();
			_jobStatisticsContainerFake = new Mock<IJobStatisticsContainer>();

			_sut = new NonDocumentJobEndMetricsService(_batchRepositoryFake.Object, _jobEndMetricsConfigurationFake.Object, _fieldManagerFake.Object, _jobStatisticsContainerFake.Object, _syncMetricsMock.Object, new EmptyLogger());
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

			const int testNumberOfFields = 10;
			IEnumerable<FieldInfoDto> fields = Enumerable.Repeat(FieldInfoDto.SupportedByViewerField(), testNumberOfFields);
			_fieldManagerFake.Setup(x => x.GetMappedFieldsNonDocumentWithoutLinksAsync(It.Is<CancellationToken>(c => c == CancellationToken.None))).ReturnsAsync(fields.ToList);

            const int testNumberOfAvailableFields = 15;
            IEnumerable<FieldInfoDto> availableFields = Enumerable.Repeat(FieldInfoDto.SupportedByViewerField(), testNumberOfAvailableFields);
            _fieldManagerFake.Setup(x => x.GetAllAvailableFieldsToMap()).Returns(availableFields.ToList);

			const ImportOverwriteMode overwriteMode = ImportOverwriteMode.AppendOnly;
			const DataSourceType sourceType = DataSourceType.SavedSearch;
			const DestinationLocationType destinationType = DestinationLocationType.Folder;

			_jobEndMetricsConfigurationFake.SetupGet(x => x.ImportOverwriteMode).Returns(overwriteMode);
			_jobEndMetricsConfigurationFake.SetupGet(x => x.DataSourceType).Returns(sourceType);
			_jobEndMetricsConfigurationFake.SetupGet(x => x.DestinationType).Returns(destinationType);

            const long jobSize = 12345;
			const long metadataSize = 6667;
			_jobStatisticsContainerFake.SetupGet(x => x.MetadataBytesTransferred).Returns(metadataSize);
			_jobStatisticsContainerFake.SetupGet(x => x.TotalBytesTransferred).Returns(jobSize);

            // Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			_syncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobEndMetric>(m =>
				m.TotalRecordsTransferred == completedItemsPerBatch * testBatches.Count &&
				m.TotalRecordsFailed == failedItemsPerBatch * testBatches.Count &&
				m.TotalRecordsRequested == totalItemsCountPerBatch * testBatches.Count &&
				m.TotalRecordsTagged == taggedItemsPerBatch * testBatches.Count &&
				m.JobEndStatus == expectedStatusDescription &&
				m.TotalMappedFields == testNumberOfFields &&
				m.TotalAvailableFields == testNumberOfAvailableFields &&
				m.BytesMetadataTransferred == metadataSize &&
				m.BytesTransferred == jobSize)), Times.Once);
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
			_syncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobEndMetric>(m => m.RetryJobEndStatus == expectedStatusDescription)), Times.Once);
		}
		
		[Test]
		public async Task ExecuteAsync_ShouldNotReportMetric_WhenJobFailed()
		{
			// Arrange
			const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;
			
			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobEndMetric>(m =>
				m.BytesMetadataTransferred == null &&
				m.BytesTransferred == null)));
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
    }
}