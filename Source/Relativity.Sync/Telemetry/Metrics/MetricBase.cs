using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal abstract class MetricBase<T> : IMetric 
		where T: IMetric
	{
		private static Dictionary<PropertyInfo, MetricAttribute> _metricCacheProperties;

		public string Name { get; }

		public string WorkflowId { get; set; }

		protected MetricBase()
		{
			Name = typeof(T).Name;
		}

		protected MetricBase(string metricName)
		{
			Name = metricName;
		}
		
		public virtual Dictionary<string, object> GetCustomData() => 
			this.GetMetricProperties().Keys.ToDictionary(p => p.Name, p => p.GetValue(this));

		public virtual IEnumerable<SumMetric> GetSumMetrics() =>
			this.GetMetricProperties()
				.Where(p => p.Value != null)
				.Select(p => new SumMetric
				{
					Type = p.Value.Type,
					Bucket = BucketNameFunc(p.Value),
					Value = p.Key.GetValue(this),
					WorkflowId = WorkflowId
				})
				.Where(m => m.Value != null);

		protected virtual string BucketNameFunc(MetricAttribute attr) => attr.Name ?? Name;

		private Dictionary<PropertyInfo, MetricAttribute> GetMetricProperties() => _metricCacheProperties ??
			(_metricCacheProperties = typeof(T)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetMethod != null)
				.ToDictionary(p => p, p => p.GetCustomAttribute<MetricAttribute>()));
	}
}
