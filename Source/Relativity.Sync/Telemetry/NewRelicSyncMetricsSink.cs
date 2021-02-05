using System.Collections.Generic;

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

			_apmClient.Log(metric.Application, customData);
		}

		private Dictionary<string, object> ReadCustomData(IMetric metric)
		{
			//TODO: Napisać logikę
			return new Dictionary<string, object>();
		}
	}
}
