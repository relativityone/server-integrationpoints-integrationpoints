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
		[Test]
		public async Task ExecuteAsyncReportsMetricAndCompletesSuccessfullyTest()
		{
			// Arrange
			var syncMetrics = new Mock<ISyncMetrics>();
			var instance = new JobStartMetricsExecutor(syncMetrics.Object);
			
			// Act
			ExecutionResult actualResult = await instance.ExecuteAsync(Mock.Of<ISumReporterConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			syncMetrics.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME), Times.Once);
		}
	}
}