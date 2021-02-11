using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	internal class TopLongTextStreamMetricTests : MetricsTestsBase<TopLongTextStreamMetric>
	{
		private TopLongTextStreamMetric _sut = new TopLongTextStreamMetric
		{
			LongTextStreamSize = 11.11,
			LongTextStreamTime = 22.22
		};

		protected override IMetric ArrangeTestMetric()
		{
			return _sut;
		}

		protected override IMetric EmptyTestMetric()
		{
			return new TopLongTextStreamMetric();
		}

		protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
		{
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_LARGEST_SIZE,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.LongTextStreamSize.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_LARGEST_TIME,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.LongTextStreamTime.Value));

			metricsManagerMock.Verify(x => x.Dispose());
			metricsManagerMock.VerifyNoOtherCalls();
		}

		protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
		{
			apmMock.Verify(x => x.Log(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
				d["LongTextStreamSize"].Equals(_sut.LongTextStreamSize) &&
				d["LongTextStreamTime"].Equals(_sut.LongTextStreamTime))));
		}
	}
}
