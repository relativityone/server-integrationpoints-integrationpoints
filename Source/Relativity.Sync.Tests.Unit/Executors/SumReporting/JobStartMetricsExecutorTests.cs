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

			const string workflowId = "workflow.id.101654";
			var configuration = new Mock<ISumReporterConfiguration>();
			configuration.SetupGet(x => x.WorkflowId).Returns(workflowId);

			// Act
			ExecutionResult actualResult = await instance.ExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			syncMetrics.Verify(x => x.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME, workflowId), Times.Once);
		}
	}
}