using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	class JobStartMetricTests : MetricTestsBase<JobStartMetric>
	{
		private JobStartMetric _sut = new JobStartMetric
		{
			Type = "Sync",
			RetryType = "Sync retry",
			FlowType = "Natives"
		};

		protected override IMetric ArrangeTestMetric()
		{
			return _sut;
		}

		protected override IMetric EmptyTestMetric()
		{
			return new JobStartMetric();
		}

		protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
		{
			metricsManagerMock.Verify(x => x.LogPointInTimeStringAsync(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.Type));
			metricsManagerMock.Verify(x => x.LogPointInTimeStringAsync(TelemetryConstants.MetricIdentifiers.RETRY_JOB_START_TYPE,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.RetryType));
			metricsManagerMock.Verify(x => x.LogPointInTimeStringAsync(TelemetryConstants.MetricIdentifiers.FLOW_TYPE,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.FlowType));

			metricsManagerMock.Verify(x => x.Dispose());
			metricsManagerMock.VerifyNoOtherCalls();
		}

		protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
		{
			apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
				d["Type"].Equals(_sut.Type) &&
				d["RetryType"].Equals(_sut.RetryType) &&
				d["FlowType"].Equals(_sut.FlowType))));
		}
	}
}
