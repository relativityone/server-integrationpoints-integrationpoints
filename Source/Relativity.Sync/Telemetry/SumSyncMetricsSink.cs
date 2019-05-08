using System;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Telemetry
{
	internal sealed class SumSyncMetricsSink : ISyncMetricsSink, IDisposable
	{
		// See comment in Dispose(bool).
		private bool _disposed = false;

		private readonly IMetricsManager _metricsManager;
		private readonly ISyncLog _logger;

		/// <summary>
		///		Creates a new instance of <see cref="NewRelicSyncMetricsSink"/>.
		/// </summary>
		/// <param name="servicesManager">Service manager, that is used to get <see cref="IMetricsManager"/></param>
		/// <param name="logger">Logger</param>
		public SumSyncMetricsSink(IServicesMgr servicesManager, ISyncLog logger)
		{
			_logger = logger;
			_metricsManager = servicesManager.CreateProxy<IMetricsManager>(ExecutionIdentity.System);
		}

		/// <inheritdoc />
		public void Log(Metric metric)
		{
			MetricType metricType = metric.Type;
			switch (metricType)
			{
				case MetricType.TimedOperation:
					LogTimedOperation(metric);
					break;
				case MetricType.Counter:
					LogCounterOperation(metric);
					break;
				case MetricType.GaugeOperation:
					LogGaugeOperation(metric);
					break;
				default:
					_logger.LogDebug("Logging metric type '{type}' to SUM is not implemented", metricType);
					break;
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		///     Sends accumulated <see cref="Metric"/>s to SUM.
		/// </summary>
		/// <param name="disposing">Indicates whether this method is being called from <see cref="Dispose()"/> (true) or from the finalizer (false).</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_metricsManager.Dispose();
				}

				// This isn't meant to ensure thread-safety, just safety from repeated calls to Dispose.
				// If this _should_ be thread-safe, consider locking or marking the field `volatile`.
				_disposed = true;
			}
		}

		private void LogGaugeOperation(Metric metric)
		{
			_metricsManager.LogGaugeAsync(metric.Name, Guid.Empty, metric.CorrelationId, (long)metric.Value);
		}

		private void LogCounterOperation(Metric metric)
		{
			_metricsManager.LogCountAsync(metric.Name, Guid.Empty, metric.CorrelationId, 1);
		}

		private void LogTimedOperation(Metric metric)
		{
			_metricsManager.LogTimerAsDoubleAsync(metric.Name, Guid.Empty, metric.CorrelationId, (double)metric.Value);
		}
	}
}