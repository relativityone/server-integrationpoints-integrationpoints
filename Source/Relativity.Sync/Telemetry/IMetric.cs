using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	internal interface IMetric
	{
		string Name { get; }

		string WorkflowId { get; set; }

		Dictionary<string, object> GetCustomData();

		IEnumerable<SumMetric> GetSumMetrics();
	}
}
