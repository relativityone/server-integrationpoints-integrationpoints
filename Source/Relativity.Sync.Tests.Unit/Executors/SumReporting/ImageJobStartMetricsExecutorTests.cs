using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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

		private Mock<IImageFileRepository> _imageFileRepositoryFake;
		private Mock<IImageJobStartMetricsConfiguration> _configurationFake;

		private IJobStatisticsContainer _jobStatisticsContainer;

		private ImageJobStartMetricsExecutor _sut;

		[SetUp]
		public void SetUp()
		{
			_syncMetricsMock = new Mock<ISyncMetrics>();

			_imageFileRepositoryFake = new Mock<IImageFileRepository>();

			_configurationFake = new Mock<IImageJobStartMetricsConfiguration>();

			_jobStatisticsContainer = new JobStatisticsContainer();

			Mock<ISyncLog> syncLog = new Mock<ISyncLog>();
			Mock<ISnapshotQueryRequestProvider> queryRequestProvider = new Mock<ISnapshotQueryRequestProvider>();

			_sut = new ImageJobStartMetricsExecutor(
				syncLog.Object,
				_syncMetricsMock.Object,
				_jobStatisticsContainer,
				_imageFileRepositoryFake.Object,
				queryRequestProvider.Object);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReportMetric()
		{
			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.Send(It.Is<JobStartMetric>(m =>
				m.Type == TelemetryConstants.PROVIDER_NAME &&
				m.FlowType == TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_IMAGES)));
		}

		[Test]
		public async Task ExecuteAsync_ShouldReportRetryMetric_WhenRetryFlowIsSelected()
		{
			// Arrange
			_configurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(100);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.Send(It.Is<JobStartMetric>(m => m.RetryType != null)));
		}

		[Test]
		public async Task ExecuteAsync_ShouldSetImagesBytesRequestedInStatisticsContainer()
		{
			// Arrange
			ImagesStatistics expectedImageStatistics = new ImagesStatistics(10, 100);

			_imageFileRepositoryFake.Setup(x =>
					x.CalculateImagesStatisticsAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<QueryImagesOptions>()))
				.ReturnsAsync(expectedImageStatistics);

			// Act
			await _sut.ExecuteAsync(_configurationFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			ImagesStatistics imageStatistics = await _jobStatisticsContainer.ImagesStatistics.ConfigureAwait(false);
			imageStatistics.Should().BeEquivalentTo(expectedImageStatistics);
		}
	}
}
