using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Logs <see cref="Metric"/>s to New Relic.
	/// </summary>
	internal sealed class NewRelicSyncMetricsSink : ISyncMetricsSink
	{
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
			_apmClient.Log(metric.Name, metric.ToDictionary());
		}
	}
}
