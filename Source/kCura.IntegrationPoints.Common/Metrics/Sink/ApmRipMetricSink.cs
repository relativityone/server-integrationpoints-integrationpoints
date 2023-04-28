using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Common.Metrics.Sink
{
    public class ApmRipMetricSink : IRipMetricsSink
    {
        private readonly IAPM _apm;

        public ApmRipMetricSink(IAPM apm)
        {
            _apm = apm;
        }

        public void Log(RipMetric ripMetric)
        {
            _apm.CountOperation(ripMetric.Name, correlationID: ripMetric.CorrelationId, customData: ripMetric.CustomData).Write();
        }
    }
}
