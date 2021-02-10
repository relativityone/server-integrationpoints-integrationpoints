using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal class MetricBase : IMetric
	{
		private readonly string _stricMetricName;

		public string Name => _stricMetricName ?? this.GetType().Name;

		public string WorkflowId { get; set; }

		public MetricBase()
		{
		}

		public MetricBase(string strictMetricName)
		{
			_stricMetricName = strictMetricName;
		}

		public IEnumerable<PropertyInfo> GetMetricProperties()
		{
			return this.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetMethod != null);
		}

	}
}
