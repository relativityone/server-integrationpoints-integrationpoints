using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	internal interface IMetric
	{
		string Name { get; }

		string CorrelationId { get; set; }

		string WorkflowId { get; set; }

		Dictionary<string, object> GetApmMetrics();

		IEnumerable<SumMetric> GetSumMetrics();
	}
}
