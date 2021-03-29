using System.Collections.Generic;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Entry point for logging metrics. Dispatches metrics to registered <see cref="ISyncMetricsSink" />s for processing.
	/// </summary>
	internal class SyncMetrics : ISyncMetrics
	{
		private readonly IEnumerable<ISyncMetricsSink> _sinks;
		private readonly IMetricsConfiguration _metricsConfiguration;
		private readonly SyncJobParameters _syncJobParameters;

		/// <summary>
		///     Creates a new instance of <see cref="SyncMetrics" /> with the given sinks.
		/// </summary>
		/// <param name="sinks">Sinks to which metrics should be sent</param>
		/// <param name="metricsConfiguration">Metrics configuration.</param>
		/// <param name="syncJobParameters">ID which correlates all metrics across a job</param>
		public SyncMetrics(IEnumerable<ISyncMetricsSink> sinks, IMetricsConfiguration metricsConfiguration, SyncJobParameters syncJobParameters)
		{
			_sinks = sinks;
			_metricsConfiguration = metricsConfiguration;
			_syncJobParameters = syncJobParameters;
		}

		/// <inheritdoc />
		public void Send(IMetric metric)
		{
			metric.WorkflowId = _syncJobParameters.WorkflowId.Value;
			metric.CorrelationId = _metricsConfiguration.CorrelationId;
			
			foreach (ISyncMetricsSink sink in _sinks)
			{
				sink.Send(metric);
			}
		}
	}
}