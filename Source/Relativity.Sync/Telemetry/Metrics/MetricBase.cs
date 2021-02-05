using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal class MetricBase : IMetric
	{
		public string Application { get; set; }

		public string WorkflowId { get; set; }

		public Dictionary<string, object> ReadCustomData() =>
			GetMetricProperties().ToDictionary(p => p.Name, p => p.GetValue(this));

		public IEnumerable<SumMetric> ReadSumMetrics()
		{
			return GetMetricProperties().Select(p =>
			{
				var attr = p.GetCustomAttribute<MetricAttribute>();

				return new SumMetric
				{
					Type = attr.Type,
					Bucket = attr.Name,
					Value = p.GetValue(this),
					WorkspaceGuid = default,
					WorkflowId = WorkflowId
				};
			});
		}

		private IEnumerable<PropertyInfo> GetMetricProperties()
		{
			return this.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetMethod != null && p.GetValue(this) != null);
		}

	}
}
