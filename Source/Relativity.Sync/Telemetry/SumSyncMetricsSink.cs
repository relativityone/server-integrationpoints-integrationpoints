using System;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Telemetry
{
	internal sealed class SumSyncMetricsSink : ISyncMetricsSink
	{
		private readonly ISyncLog _logger;
		private readonly IServicesMgr _servicesManager;

		/// <summary>
		///		Creates a new instance of <see cref="NewRelicSyncMetricsSink"/>.
		/// </summary>
		/// <param name="servicesManager">Service manager, that is used to get <see cref="IMetricsManager"/></param>
		/// <param name="logger">Logger</param>
		public SumSyncMetricsSink(IServicesMgr servicesManager, ISyncLog logger)
		{
			_logger = logger;
			_servicesManager = servicesManager;
		}

		private IMetricsManager CreateMetricsManager()
		{
			return _servicesManager.CreateProxy<IMetricsManager>(ExecutionIdentity.System);
		}

		/// <inheritdoc />
		public void Log(Metric metric)
		{
			using (IMetricsManager metricManager = CreateMetricsManager())
			{
				LogSumMetric(metricManager, metric);
			}
		}

		private void LogSumMetric(IMetricsManager metricsManager, Metric metric)
		{
			MetricType metricType = metric.Type;
			switch (metricType)
			{
				case MetricType.TimedOperation:
					LogTimedOperation(metricsManager, metric);
					break;
				case MetricType.Counter:
					LogCounterOperation(metricsManager, metric);
					break;
				case MetricType.GaugeOperation:
					LogGaugeOperation(metricsManager, metric);
					break;
				default:
					_logger.LogDebug("Logging metric type '{type}' to SUM is not implemented", metricType);
					break;
			}
		}

		private static void LogGaugeOperation(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogGaugeAsync(metric.Name, Guid.Empty, metric.CorrelationId, (long)metric.Value);
		}

		private static void LogCounterOperation(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogCountAsync(metric.Name, Guid.Empty, metric.CorrelationId, 1);
		}

		private static void LogTimedOperation(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogTimerAsDoubleAsync(metric.Name, Guid.Empty, metric.CorrelationId, (double)metric.Value);
		}
	}
}