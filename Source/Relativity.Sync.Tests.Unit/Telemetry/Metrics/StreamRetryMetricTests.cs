using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	internal class StreamRetryMetricTests : MetricTestsBase<StreamRetryMetric>
	{
		private StreamRetryMetric _sut = new StreamRetryMetric
		{
			RetryCounter = Counter.Increment
		};

		protected override IMetric ArrangeTestMetric()
		{
			return _sut;
		}

		protected override IMetric EmptyTestMetric()
		{
			return new StreamRetryMetric();
		}

		protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
		{
			metricsManagerMock.Verify(x => x.LogCountAsync(TelemetryConstants.MetricIdentifiers.LONG_TEXT_STREAM_RETRY_COUNT,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, 1));

			metricsManagerMock.Verify(x => x.Dispose());
		}

		protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
		{
			apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
				d["RetryCounter"].Equals(_sut.RetryCounter.ToString()))));
		}
	}
}
