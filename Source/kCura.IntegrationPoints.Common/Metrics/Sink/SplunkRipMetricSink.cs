using Relativity.API;

namespace kCura.IntegrationPoints.Common.Metrics.Sink
{
	public class SplunkRipMetricSink : IRipMetricsSink
	{
		private readonly IAPILog _logger;

		public SplunkRipMetricSink(IAPILog logger)
		{
			_logger = logger;
		}

		public void Log(RipMetric ripMetric)
		{
			_logger.LogInformation("Logging metric '{MetricName}' with value: {value} and properties: {@MetricProperties}",
				ripMetric.Name, ripMetric.Value, ripMetric);
		}
	}
}