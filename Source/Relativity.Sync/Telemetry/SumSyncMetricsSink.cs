using System;
using System.Collections.Generic;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Telemetry
{
	internal sealed class SumSyncMetricsSink : ISyncMetricsSink, IDisposable
	{
		private bool _disposed;

		private readonly IMetricsManager _metricsManager;
		private readonly List<Metric> _metrics;
		private readonly ISyncLog _logger;

		public SumSyncMetricsSink(IServicesMgr servicesManager, ISyncLog logger)
		{
			_logger = logger;
			_metricsManager = servicesManager.CreateProxy<IMetricsManager>(ExecutionIdentity.System);
			_metrics = new List<Metric>();
		}

		public void Log(Metric metric)
		{
			_metrics.Add(metric);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_metrics.ForEach(LogSumMetric);
					_metricsManager.Dispose();
				}

				// This isn't meant to ensure thread-safety, just safety from repeated calls to Dispose.
				// If this _should_ be thread-safe, consider locking or marking the field `volatile`.
				_disposed = true;
			}
		}

		private void LogSumMetric(Metric metric)
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