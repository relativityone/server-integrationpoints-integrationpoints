using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	internal interface IMetric
	{
		string Name { get; }

		string CorrelationId { get; set; }
		
		string ExecutingApplication { get; set;  }
		
		string ExecutingApplicationVersion { get; set; }
		
		string SyncVersion { get; set; }

		string DataSourceType { get; set; }

		string DataDestinationType { get; set; }

		string FlowName { get; set; }

		bool IsRetry { get; set; }

        Dictionary<string, object> GetApmMetrics();

		IEnumerable<SumMetric> GetSumMetrics();
	}
}
