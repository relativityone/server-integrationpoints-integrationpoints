using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	public class JobStartMetricsExecutorTests
	{
		private JobStartMetricsExecutor _sut;

		private Mock<ISyncMetrics> _syncMetricsMock;
		private Mock<ISumReporterConfiguration> _sumReporterConfigurationFake;

		[SetUp]
		public void SetUp()
		{
			_syncMetricsMock = new Mock<ISyncMetrics>();
			_sumReporterConfigurationFake = new Mock<ISumReporterConfiguration>();

			_sut = new JobStartMetricsExecutor(_syncMetricsMock.Object);
		}

		[Test]
		public async Task ExecuteAsyncReportsMetricAndCompletesSuccessfullyTest()
		{
			// Act
			ExecutionResult actualResult = await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME), Times.Once);
		}

		[Test]
		public async Task ExecuteAsync_ShouldSendStartRetryMetric_WhenJobHasBeenRetried()
		{
			// Arrange
			_sumReporterConfigurationFake.Setup(x => x.JobHistoryToRetryId).Returns(It.IsAny<int>());

			// Act
			await _sut.ExecuteAsync(_sumReporterConfigurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.RETRY_JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME), Times.Once);
		}
	}
}