using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal abstract class MetricBase : IMetric
	{
		private static Dictionary<PropertyInfo, MetricAttribute> _metricProperties;

		public string Name { get; }

		public string WorkflowId { get; set; }

		protected MetricBase()
		{
			Name = this.GetType().Name;
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
					Bucket = p.Value.Name ?? Name,
					Value = p.Key.GetValue(this),
					WorkflowId = WorkflowId
				});

		private Dictionary<PropertyInfo, MetricAttribute> GetMetricProperties() => _metricProperties ??
			(_metricProperties = this.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetMethod != null)
				.ToDictionary(p => p, p => p.GetCustomAttribute<MetricAttribute>()));
	}
}
