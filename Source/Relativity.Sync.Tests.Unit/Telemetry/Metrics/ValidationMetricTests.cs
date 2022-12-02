using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
    internal class ValidationMetricTests : MetricTestsBase<ValidationMetric>
    {
        private const string _VALIDATION_NAME = "TestValidation";

        private ValidationMetric _sut = new ValidationMetric(_VALIDATION_NAME)
        {
            ExecutionStatus = ExecutionStatus.Canceled,
            Duration = 11.11,
            FailedCounter = Counter.Increment
        };

        protected override IMetric ArrangeTestMetric()
        {
            return _sut;
        }

        protected override IMetric EmptyTestMetric()
        {
            return new ValidationMetric(_VALIDATION_NAME);
        }

        protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
        {
            metricsManagerMock.Verify(x => x.LogTimerAsDoubleAsync(_VALIDATION_NAME, _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.Duration.Value));
            metricsManagerMock.Verify(x => x.LogCountAsync(_VALIDATION_NAME, _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, 1));

            metricsManagerMock.Verify(x => x.Dispose());
        }

        protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
        {
            apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
                d["ExecutionStatus"].Equals(_sut.ExecutionStatus.ToString()) &&
                d["Duration"].Equals(_sut.Duration) &&
                d["FailedCounter"].Equals(_sut.FailedCounter.ToString()))));
        }
    }
}
