namespace Relativity.Sync.Telemetry.Metrics
{
	internal abstract class PrefixedMetricBase : MetricBase
	{
		protected PrefixedMetricBase(string prefix)
		{
			Name = prefix;
		}
		
		protected override string BucketFunc(MetricAttribute attr) => attr.Name != null ? $"{Name}.{attr.Name}" : Name;
	}
}
