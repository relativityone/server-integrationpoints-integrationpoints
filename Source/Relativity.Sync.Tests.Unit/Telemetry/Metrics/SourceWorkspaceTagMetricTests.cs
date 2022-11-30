using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
    class SourceWorkspaceTagMetricTests : MetricTestsBase<SourceWorkspaceTagMetric>
    {
        private SourceWorkspaceTagMetric _sut = new SourceWorkspaceTagMetric
        {
            DestinationUpdateTime = 11.1,
            DestinationUpdateCount = 22,
            UnitOfMeasure = "document(s)",
            BatchSize = 33
        };

        protected override IMetric ArrangeTestMetric()
        {
            return _sut;
        }

        protected override IMetric EmptyTestMetric()
        {
            return new SourceWorkspaceTagMetric();
        }

        protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
        {
            metricsManagerMock.Verify(x => x.LogTimerAsDoubleAsync(
                TelemetryConstants.MetricIdentifiers.TAG_DOCUMENTS_DESTINATION_UPDATE_TIME,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.DestinationUpdateTime.Value));
            metricsManagerMock.Verify(x => x.LogGaugeAsync(
                TelemetryConstants.MetricIdentifiers.TAG_DOCUMENTS_DESTINATION_UPDATE_COUNT,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.DestinationUpdateCount.Value));

            metricsManagerMock.Verify(x => x.Dispose());
        }

        protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
        {
            apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
                d["DestinationUpdateTime"].Equals(_sut.DestinationUpdateTime) &&
                d["DestinationUpdateCount"].Equals(_sut.DestinationUpdateCount) &&
                d["UnitOfMeasure"].Equals(_sut.UnitOfMeasure) &&
                d["BatchSize"].Equals(_sut.BatchSize))));
        }
    }
}
