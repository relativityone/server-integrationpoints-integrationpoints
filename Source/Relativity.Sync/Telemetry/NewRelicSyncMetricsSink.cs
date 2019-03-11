using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Logs <see cref="Metric"/>s to New Relic. Metrics are aggregated and sent in a single call
	///     when the object is disposed, which should be at the end of the Sync job.
	/// </summary>
	internal sealed class NewRelicSyncMetricsSink : ISyncMetricsSink, IDisposable
	{
		// See comment in Dispose(bool).
		private bool _disposed = false;

		private const string _METRIC_NAME = "Relativity.Sync.JobComplete";

		private readonly IAPMClient _apmClient;
		private readonly List<Metric> _metrics;

		/// <summary>
		///     Creates a new instance of <see cref="NewRelicSyncMetricsSink"/>.
		/// </summary>
		/// <param name="apmClient">APM to use for logging metrics</param>
		public NewRelicSyncMetricsSink(IAPMClient apmClient)
		{
			_apmClient = apmClient;
			_metrics = new List<Metric>();
		}

		/// <inheritdoc />
		public void Log(Metric metric)
		{
			_metrics.Add(metric);
		}

		/// <summary>
		///     Sends accumulated <see cref="Metric"/>s to the APM endpoint.
		/// </summary>
		/// <param name="disposing">Indicates whether this method is being called from <see cref="Dispose()"/> (true) or from the finalizer (false).</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Dictionary<string, object> payload = BuildPayload(_metrics);
					_apmClient.Log(_METRIC_NAME, payload);
				}

				// This isn't meant to ensure thread-safety, just safety from repeated calls to Dipose.
				// If this _should_ be thread-safe, consider locking or marking the field `volatile`.
				_disposed = true;
			}
		}

		/// <summary>
		///     Builds a customData payload for APM so we can send all metrics in one call.
		///     Keys are random IDs and values are dictionaries mapping public Metric properties
		///     to their values, i.e.: {(guid) => {(property) => (property_val)}}
		/// </summary>
		private static Dictionary<string, object> BuildPayload(IEnumerable<Metric> metrics)
		{
			Dictionary<string, object> payload = metrics.ToDictionary(m => Guid.NewGuid().ToString(), m => (object)m.ToDictionary());
			return payload;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
		}
	}
}
