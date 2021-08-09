namespace Relativity.Sync.Telemetry
{
	internal class SumMetric
	{
		public string CorrelationId { get; set; }

		public MetricType Type { get; set; }

		public string Bucket { get; set; }

		public object Value { get; set; }
	}
}
