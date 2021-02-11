using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal abstract class MetricBase : IMetric
	{
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
			this.GetMetricProperties().ToDictionary(p => p.Name, p => p.GetValue(this));

		public virtual IEnumerable<SumMetric> GetSumMetrics()
		{
			return this.GetMetricProperties()
				.Where(p => p.GetCustomAttribute<MetricAttribute>() != null && p.GetValue(this) != null)
				.Select(p =>
				{
					var attr = p.GetCustomAttribute<MetricAttribute>();

					return new SumMetric
					{
						Type = attr.Type,
						Bucket = attr.Name ?? Name,
						Value = p.GetValue(this),
						WorkflowId = WorkflowId
					};
				}).ToList();
		}

		private IEnumerable<PropertyInfo> GetMetricProperties()
		{
			return this.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetMethod != null);
		}
	}
}
