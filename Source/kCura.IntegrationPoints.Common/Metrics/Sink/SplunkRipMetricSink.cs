using Relativity.API;

namespace kCura.IntegrationPoints.Common.Metrics.Sink
{
    public class SplunkRipMetricSink : IRipMetricsSink
    {
        private readonly ILogger<SplunkRipMetricSink> _logger;

        public SplunkRipMetricSink(ILogger<SplunkRipMetricSink> logger)
        {
            _logger = logger;
        }

        public void Log(RipMetric ripMetric)
        {
            _logger.LogInformation("Logging metric '{MetricName}' with properties: {@MetricProperties}", ripMetric.Name, ripMetric);
        }
    }
}
