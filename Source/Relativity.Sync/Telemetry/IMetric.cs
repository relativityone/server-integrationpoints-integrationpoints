using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	internal interface IMetric
	{
		string Name { get; }

		string WorkflowId { get; set; }
		
		string ExecutingApplication { get; set;  }
		
		string ExecutingApplicationVersion { get; set; }

		Dictionary<string, object> GetApmMetrics();

		IEnumerable<SumMetric> GetSumMetrics();
	}
}
