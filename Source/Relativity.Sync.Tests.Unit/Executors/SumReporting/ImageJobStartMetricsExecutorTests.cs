using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
    [TestFixture]
    internal class ImageJobStartMetricsExecutorTests
    {
        private Mock<ISyncMetrics> _syncMetricsMock;

        private Mock<IFileStatisticsCalculator> _fileStatisticsCalculatorFake;
        private Mock<IImageJobStartMetricsConfiguration> _configurationFake;

        private IJobStatisticsContainer _jobStatisticsContainer;

        private ImageJobStartMetricsExecutor _sut;

        [SetUp]
        public void SetUp()
        {
            _syncMetricsMock = new Mock<ISyncMetrics>();

            _fileStatisticsCalculatorFake = new Mock<IFileStatisticsCalculator>();

            _configurationFake = new Mock<IImageJobStartMetricsConfiguration>();

            _jobStatisticsContainer = new JobStatisticsContainer();

            Mock<IAPILog> syncLog = new Mock<IAPILog>();
            Mock<ISnapshotQueryRequestProvider> queryRequestProvider = new Mock<ISnapshotQueryRequestProvider>();

            _sut = new ImageJobStartMetricsExecutor(
                syncLog.Object,
                _syncMetricsMock.Object,
                _jobStatisticsContainer,
                _fileStatisticsCalculatorFake.Object,
                queryRequestProvider.Object);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReportJobStartMetric()
        {
            // Act
            await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _syncMetricsMock.Verify(x => x.Send(It.Is<JobStartMetric>(m =>
                m.Type == TelemetryConstants.PROVIDER_NAME &&
                m.FlowType == TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_IMAGES)));
            _syncMetricsMock.Verify(x => x.Send(It.IsAny<JobResumeMetric>()), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReportJobStartRetryMetric_WhenRetryFlowIsSelected()
        {
            // Arrange
            _configurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(100);

            // Act
            await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _syncMetricsMock.Verify(x => x.Send(It.Is<JobStartMetric>(m => m.RetryType != null)));
            _syncMetricsMock.Verify(x => x.Send(It.IsAny<JobResumeMetric>()), Times.Never);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ExecuteAsync_ShouldAlwaysSetImagesBytesRequestedInStatisticsContainer(bool isResuming)
        {
            // Arrange
            ImagesStatistics expectedImageStatistics = new ImagesStatistics(10, 100);

            _configurationFake.SetupGet(x => x.Resuming).Returns(isResuming);

            _fileStatisticsCalculatorFake.Setup(x =>
                    x.CalculateImagesStatisticsAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(),
                        It.IsAny<QueryImagesOptions>(), It.IsAny<CompositeCancellationToken>()))
                .ReturnsAsync(expectedImageStatistics);

            // Act
            await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            ImagesStatistics imageStatistics = await _jobStatisticsContainer.ImagesStatistics.ConfigureAwait(false);
            imageStatistics.Should().BeEquivalentTo(expectedImageStatistics);
        }

        [Test]
        public async Task ExecuteAsync_ShouldSendJobResumeMetric_WhenResuming()
        {
            // Arrange
            _configurationFake.SetupGet(x => x.Resuming).Returns(true);

            // Act
            await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _syncMetricsMock.Verify(x => x.Send(It.Is<JobResumeMetric>(metric =>
                metric.Type == TelemetryConstants.PROVIDER_NAME)), Times.Once);
            _syncMetricsMock.Verify(x => x.Send(It.IsAny<JobStartMetric>()), Times.Never);
        }
    }
}
