using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	internal class DestinationWorkspaceTagMetricTests : MetricTestsBase<DestinationWorkspaceTagMetric>
	{
		private DestinationWorkspaceTagMetric _sut = new DestinationWorkspaceTagMetric
		{
			SourceUpdateTime = 111.11,
			SourceUpdateCount = 1111,
			UnitOfMeasure = "document(s)",
			BatchSize = 11
		};

		protected override IMetric ArrangeTestMetric()
		{
			return _sut;
		}

		protected override IMetric EmptyTestMetric()
		{
			return new DestinationWorkspaceTagMetric();
		}

		protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
		{
			metricsManagerMock.Verify(x => x.LogTimerAsDoubleAsync(TelemetryConstants.MetricIdentifiers.TAG_DOCUMENTS_SOURCE_UPDATE_TIME, 
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.SourceUpdateTime.Value));
			metricsManagerMock.Verify(x => x.LogGaugeAsync(TelemetryConstants.MetricIdentifiers.TAG_DOCUMENTS_SOURCE_UPDATE_COUNT,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.SourceUpdateCount.Value));

			metricsManagerMock.Verify(x => x.Dispose());
			metricsManagerMock.VerifyNoOtherCalls();
		}

		protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
		{
			apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
				d["SourceUpdateTime"].Equals(_sut.SourceUpdateTime) &&
				d["SourceUpdateCount"].Equals(_sut.SourceUpdateCount) &&
				d["UnitOfMeasure"].Equals(_sut.UnitOfMeasure) &&
				d["BatchSize"].Equals(_sut.BatchSize))));
		}
	}
}
