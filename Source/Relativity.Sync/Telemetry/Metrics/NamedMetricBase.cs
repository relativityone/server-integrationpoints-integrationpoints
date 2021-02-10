namespace Relativity.Sync.Telemetry.Metrics
{
	internal abstract class NamedMetricBase : MetricBase
	{
		protected NamedMetricBase(string name)
		{
			Name = name;
		}

		protected override string BucketFunc(MetricAttribute attr) => Name;
	}
}
