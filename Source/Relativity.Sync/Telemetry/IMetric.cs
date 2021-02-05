using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	internal interface IMetric
	{
		string Application { get; set; }

		Dictionary<string, object> ReadCustomData();

		IEnumerable<SumMetric> ReadSumMetrics();
	}
}
