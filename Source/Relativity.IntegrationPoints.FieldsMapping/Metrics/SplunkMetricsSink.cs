using Relativity.API;

namespace Relativity.IntegrationPoints.FieldsMapping.Metrics
{
    public class SplunkMetricsSink : IMetricsSink
    {
        private readonly IAPILog _logger;

        public SplunkMetricsSink(IAPILog logger)
        {
            _logger = logger;
        }

        public void Log(Metric metric)
        {
            _logger.LogInformation("Logging metric '{MetricName}' with properties: {@MetricProperties}", metric.Name, metric);
        }
    }
}