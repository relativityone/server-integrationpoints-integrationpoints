using System.Collections.Generic;
using System.Reflection;

namespace Relativity.Sync.Telemetry
{
	internal interface IMetric
	{
		string Application { get; set; }

		string WorkflowId { get; set; }

		IEnumerable<PropertyInfo> GetMetricProperties();
	}
}
