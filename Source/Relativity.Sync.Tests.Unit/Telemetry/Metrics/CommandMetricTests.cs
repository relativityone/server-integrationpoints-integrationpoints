using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
    internal class CommandMetricTests : MetricTestsBase<CommandMetric>
    {
        private const string _COMMAND_NAME = "TestCommand";

        private CommandMetric _sut = new CommandMetric(_COMMAND_NAME)
        {
            Duration = 1111,
            ExecutionStatus = ExecutionStatus.Completed,
        };


        protected override IMetric ArrangeTestMetric()
        {
            return _sut;
        }

        protected override IMetric EmptyTestMetric()
        {
            return new CommandMetric(_COMMAND_NAME);
        }

        protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
        {
            metricsManagerMock.Verify(x => x.LogTimerAsDoubleAsync(_COMMAND_NAME, _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.Duration.Value));

            metricsManagerMock.Verify(x => x.Dispose());
        }

        protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
        {
            apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
                d["Duration"].Equals(_sut.Duration) &&
                d["ExecutionStatus"].Equals(_sut.ExecutionStatus.ToString()))));
        }
    }
}
