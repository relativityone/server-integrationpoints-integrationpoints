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
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	public class NonDocumentJobEndMetricsServiceTests : JobEndMetricsServiceTestsBase
	{
		private NonDocumentJobEndMetricsService _sut;

        [SetUp]
		public override void SetUp()
		{
			base.SetUp();

            FieldManagerFake.Setup(x => x.GetMappedFieldsNonDocumentForLinksAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FieldInfoDto>().AsReadOnly());
			FieldManagerFake.Setup(x => x.GetMappedFieldsNonDocumentWithoutLinksAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<FieldInfoDto>().AsReadOnly());
            FieldManagerFake.Setup(x => x.GetAllAvailableFieldsToMap())
                .Returns(new List<FieldInfoDto>());
			
			_sut = new NonDocumentJobEndMetricsService(BatchRepositoryFake.Object, JobEndMetricsConfigurationFake.Object, FieldManagerFake.Object, JobStatisticsContainerFake.Object, SyncMetricsMock.Object, new EmptyLogger());
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
			BatchRepositoryFake.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(testBatches);

            const int testNumberOfLinkedFields = 8;
            IEnumerable<FieldInfoDto> linkedFields = Enumerable.Repeat(FieldInfoDto.SupportedByViewerField(), testNumberOfLinkedFields);
            FieldManagerFake.Setup(x => x.GetMappedFieldsNonDocumentForLinksAsync(It.Is<CancellationToken>(c => c == CancellationToken.None))).ReturnsAsync(linkedFields.ToList);

			const int testNumberOfFields = 10;
			IEnumerable<FieldInfoDto> fields = Enumerable.Repeat(FieldInfoDto.SupportedByViewerField(), testNumberOfFields);
			FieldManagerFake.Setup(x => x.GetMappedFieldsNonDocumentWithoutLinksAsync(It.Is<CancellationToken>(c => c == CancellationToken.None))).ReturnsAsync(fields.ToList);

            const int testNumberOfAvailableFields = 15;
            IEnumerable<FieldInfoDto> availableFields = Enumerable.Repeat(FieldInfoDto.SupportedByViewerField(), testNumberOfAvailableFields);
            FieldManagerFake.Setup(x => x.GetAllAvailableFieldsToMap()).Returns(availableFields.ToList);

			const ImportOverwriteMode overwriteMode = ImportOverwriteMode.AppendOnly;
			const DataSourceType sourceType = DataSourceType.SavedSearch;
			const DestinationLocationType destinationType = DestinationLocationType.Folder;

			JobEndMetricsConfigurationFake.SetupGet(x => x.ImportOverwriteMode).Returns(overwriteMode);
			JobEndMetricsConfigurationFake.SetupGet(x => x.DataSourceType).Returns(sourceType);
			JobEndMetricsConfigurationFake.SetupGet(x => x.DestinationType).Returns(destinationType);

            const long jobSize = 12345;
			const long metadataSize = 6667;
			JobStatisticsContainerFake.SetupGet(x => x.MetadataBytesTransferred).Returns(metadataSize);
			JobStatisticsContainerFake.SetupGet(x => x.TotalBytesTransferred).Returns(jobSize);

            // Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			SyncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobEndMetric>(m =>
				m.TotalRecordsTransferred == completedItemsPerBatch * testBatches.Count &&
				m.TotalRecordsFailed == failedItemsPerBatch * testBatches.Count &&
				m.TotalRecordsRequested == totalItemsCountPerBatch * testBatches.Count &&
				m.TotalRecordsTagged == taggedItemsPerBatch * testBatches.Count &&
				m.JobEndStatus == expectedStatusDescription &&
				m.TotalMappedFields == testNumberOfFields &&
				m.TotalLinksMappedFields == testNumberOfLinkedFields &&
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

			JobEndMetricsConfigurationFake.Setup(x => x.JobHistoryToRetryId).Returns(It.IsAny<int>());

			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			SyncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobEndMetric>(m => m.RetryJobEndStatus == expectedStatusDescription)), Times.Once);
		}
		
		[Test]
		public async Task ExecuteAsync_ShouldNotReportMetric_WhenJobFailed()
		{
			// Arrange
			const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;
			
			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			SyncMetricsMock.Verify(x => x.Send(It.Is<NonDocumentJobEndMetric>(m =>
				m.BytesMetadataTransferred == null &&
				m.BytesTransferred == null)));
		}

		[Test]
		public async Task ExecuteAsync_ShouldBeCompleted_WhenGetBatchesThrowsException()
		{
			// Arrange
			const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

			BatchRepositoryFake.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).Throws<SyncException>();

			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

			// Assert
			actualResult.Should().NotBeNull();
			actualResult.Status.Should().Be(ExecutionStatus.Completed);
		}
    }
}