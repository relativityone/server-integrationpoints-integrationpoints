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
    public class DocumentJobEndMetricsServiceTests : JobEndMetricsServiceTestsBase
    {
        private DocumentJobEndMetricsService _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            FieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FieldInfoDto>().AsReadOnly());
            JobStatisticsContainerFake
                .Setup(x => x.CalculateAverageLongTextStreamSizeAndTime(It.IsAny<Func<long, bool>>()))
                .Returns(new Tuple<double, double>(1, 2));
            JobStatisticsContainerFake
                .SetupGet(x => x.LongTextStatistics)
                .Returns(Enumerable.Range(1, 20).Select(x => new LongTextStreamStatistics()
                {
                    TotalBytesRead = x * 1024 * 1024,
                    TotalReadTime = TimeSpan.FromSeconds(x)
                }).ToList());

            _sut = new DocumentJobEndMetricsService(BatchRepositoryFake.Object, JobEndMetricsConfigurationFake.Object, FieldManagerFake.Object, JobStatisticsContainerFake.Object, SyncMetricsMock.Object, new EmptyLogger());
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

            const int testNumberOfFields = 10;
            IEnumerable<FieldInfoDto> fields = Enumerable.Repeat(FieldInfoDto.NativeFileFilenameField(), testNumberOfFields);
            FieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.Is<CancellationToken>(c => c == CancellationToken.None))).ReturnsAsync(fields.ToList);

            const ImportOverwriteMode overwriteMode = ImportOverwriteMode.AppendOnly;
            const DataSourceType sourceType = DataSourceType.SavedSearch;
            const DestinationLocationType destinationType = DestinationLocationType.Folder;
            const ImportNativeFileCopyMode nativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles;

            JobEndMetricsConfigurationFake.SetupGet(x => x.ImportOverwriteMode).Returns(overwriteMode);
            JobEndMetricsConfigurationFake.SetupGet(x => x.DataSourceType).Returns(sourceType);
            JobEndMetricsConfigurationFake.SetupGet(x => x.DestinationType).Returns(destinationType);
            JobEndMetricsConfigurationFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(nativeFileCopyMode);

            const long jobSize = 12345;
            const long metadataSize = 6667;
            const long nativesSize = 5678;
            JobStatisticsContainerFake.SetupGet(x => x.MetadataBytesTransferred).Returns(metadataSize);
            JobStatisticsContainerFake.SetupGet(x => x.FilesBytesTransferred).Returns(nativesSize);
            JobStatisticsContainerFake.SetupGet(x => x.TotalBytesTransferred).Returns(jobSize);
            JobStatisticsContainerFake.SetupGet(x => x.NativesBytesRequested).Returns(Task.FromResult(nativesSize));

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Status.Should().Be(ExecutionStatus.Completed);

            SyncMetricsMock.Verify(
                x => x.Send(It.Is<DocumentJobEndMetric>(m =>
                m.TotalRecordsTransferred == completedItemsPerBatch * testBatches.Count &&
                m.TotalRecordsFailed == failedItemsPerBatch * testBatches.Count &&
                m.TotalRecordsRequested == totalItemsCountPerBatch * testBatches.Count &&
                m.TotalRecordsTagged == taggedItemsPerBatch * testBatches.Count &&
                m.JobEndStatus == expectedStatusDescription &&
                m.TotalMappedFields == testNumberOfFields &&
                m.BytesMetadataTransferred == metadataSize &&
                m.BytesNativesTransferred == nativesSize &&
                m.BytesTransferred == jobSize &&
                m.BytesNativesRequested == nativesSize)), Times.Once);
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
            SyncMetricsMock.Verify(x => x.Send(It.Is<DocumentJobEndMetric>(m => m.RetryJobEndStatus == expectedStatusDescription)), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldNotLogBytesTransferred_WhenNoNativeFilesHasBeenTransferred()
        {
            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(ExecutionStatus.Completed).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Status.Should().Be(ExecutionStatus.Completed);
            SyncMetricsMock.Verify(x => x.Send(It.Is<DocumentJobEndMetric>(m => m.BytesNativesTransferred == null)));
        }

        [Test]
        public async Task ExecuteAsync_ShouldNotReportMetric_WhenJobFailed()
        {
            // Arrange
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            SyncMetricsMock.Verify(x => x.Send(It.Is<DocumentJobEndMetric>(m =>
                m.BytesMetadataTransferred == null &&
                m.BytesNativesTransferred == null &&
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

        [Test]
        public async Task ExecuteAsync_ShouldSendJobEndStatusMetric_WhenNativesBytesRequestedIsNull()
        {
            // Arrange
            const string expectedStatusDescription = "Completed with Errors";
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;

            JobStatisticsContainerFake.SetupGet(x => x.NativesBytesRequested).Returns((Task<long>)null);

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Status.Should().Be(ExecutionStatus.Completed);
            SyncMetricsMock.Verify(x => x.Send(It.Is<DocumentJobEndMetric>(m => m.JobEndStatus == expectedStatusDescription)), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldSendJobEndStatusMetric_WhenNativesBytesRequestedThrowsException()
        {
            // Arrange
            const string expectedStatusDescription = "Completed with Errors";
            const ExecutionStatus expectedStatus = ExecutionStatus.CompletedWithErrors;
            Exception exception = new Exception();

            JobStatisticsContainerFake.SetupGet(x => x.NativesBytesRequested).Throws(exception);

            // Act
            ExecutionResult actualResult = await _sut.ExecuteAsync(expectedStatus).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Status.Should().Be(ExecutionStatus.Completed);
            SyncMetricsMock.Verify(x => x.Send(It.Is<DocumentJobEndMetric>(m => m.JobEndStatus == expectedStatusDescription)), Times.Once);
        }
    }
}
