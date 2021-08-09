using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	internal class KeplerMetricTests : MetricTestsBase<KeplerMetric>
	{
		private const string _KEPLER_NAME = "ObjectManager";

		private KeplerMetric _sut = new KeplerMetric(_KEPLER_NAME)
		{
			ExecutionStatus = ExecutionStatus.Completed,
			Duration = 111.11,
			NumberOfHttpRetriesForSuccess = 2,
			NumberOfHttpRetriesForFailed = 3,
			AuthTokenExpirationCount = 4
		};

		protected override IMetric ArrangeTestMetric()
		{
			return _sut;
		}

		protected override IMetric EmptyTestMetric()
		{
			return new KeplerMetric(_KEPLER_NAME);
		}

		protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
		{
			metricsManagerMock.Verify(x => x.LogTimerAsDoubleAsync(
				$"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{_KEPLER_NAME}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_DURATION_SUFFIX}",
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.Duration.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
				$"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{_KEPLER_NAME}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_SUCCESS_SUFFIX}",
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.NumberOfHttpRetriesForSuccess.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
				$"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{_KEPLER_NAME}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_FAILED_SUFFIX}",
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.NumberOfHttpRetriesForFailed.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
				$"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{_KEPLER_NAME}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_AUTH_REFRESH_SUFFIX}",
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AuthTokenExpirationCount.Value));

			metricsManagerMock.Verify(x => x.Dispose());
			metricsManagerMock.VerifyNoOtherCalls();
		}

		protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
		{
			apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
				d["ExecutionStatus"].Equals(_sut.ExecutionStatus.ToString()) &&
				d["Duration"].Equals(_sut.Duration) &&
				d["NumberOfHttpRetriesForSuccess"].Equals(_sut.NumberOfHttpRetriesForSuccess) &&
				d["NumberOfHttpRetriesForFailed"].Equals(_sut.NumberOfHttpRetriesForFailed) &&
				d["AuthTokenExpirationCount"].Equals(_sut.AuthTokenExpirationCount))));
		}
	}
}
