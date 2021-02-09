using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Logs <see cref="Metric"/>s to New Relic.
	/// </summary>
	internal sealed class NewRelicSyncMetricsSink : ISyncMetricsSink
	{
		private const string _NEW_RELIC_INDEX_NAME = "Relativity.Sync";

		private readonly IAPMClient _apmClient;

		/// <summary>
		///     Creates a new instance of <see cref="NewRelicSyncMetricsSink"/>.
		/// </summary>
		/// <param name="apmClient">APM to use for logging metrics</param>
		public NewRelicSyncMetricsSink(IAPMClient apmClient)
		{
			_apmClient = apmClient;
		}

		/// <inheritdoc />
		public void Log(Metric metric)
		{
			_apmClient.Log(_NEW_RELIC_INDEX_NAME, metric.ToDictionary());
		}

		public void Send(IMetric metric)
		{
			var customData = ReadCustomData(metric);
			if (customData.Count > 0)
			{
				_apmClient.Log(_NEW_RELIC_INDEX_NAME, customData);
			}
		}

		private Dictionary<string, object> ReadCustomData(IMetric metric) =>
			metric.GetMetricProperties().ToDictionary(p => p.Name, p => p.GetValue(metric));
	}
}
