using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Entry point for logging metrics. Dispatches metrics to registered <see cref="ISyncMetricsSink" />s for processing.
	/// </summary>
	internal class SyncMetrics : ISyncMetrics
	{
		private readonly IEnumerable<ISyncMetricsSink> _sinks;
		private readonly SyncJobParameters _syncJobParameters;

		/// <summary>
		///     Creates a new instance of <see cref="SyncMetrics" /> with the given sinks.
		/// </summary>
		/// <param name="sinks">Sinks to which metrics should be sent</param>
		/// <param name="syncJobParameters">ID which correlates all metrics across a job</param>
		public SyncMetrics(IEnumerable<ISyncMetricsSink> sinks, SyncJobParameters syncJobParameters)
		{
			_sinks = sinks;
			_syncJobParameters = syncJobParameters;
		}

		/// <inheritdoc />
		public void Send(IMetric metric)
		{
			metric.WorkflowId = _syncJobParameters.WorkflowId.Value;
			metric.ExecutingApplication = _syncJobParameters.ExecutingApplication;
			metric.ExecutingApplicationVersion = _syncJobParameters.ExecutingApplicationVersion;

			foreach (ISyncMetricsSink sink in _sinks)
			{
				sink.Send(metric);
			}
		}
	}
}