using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal class MetricBase : IMetric
	{
		public string Application { get; set; }

		public string WorkflowId { get; set; }
		
		public IEnumerable<PropertyInfo> GetMetricProperties()
		{
			return this.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetMethod != null && p.GetValue(this) != null);
		}

	}
}
