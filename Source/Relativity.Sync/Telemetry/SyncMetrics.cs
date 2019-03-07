using System;
using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Entry point for logging metrics. Dispatches metrics to registered <see cref="ISyncMetricsSink"/>s for processing.
	/// </summary>
	internal class SyncMetrics : ISyncMetrics
	{
		private readonly IEnumerable<ISyncMetricsSink> _sinks;

		/// <summary>
		///     Creates a new instance of <see cref="SyncMetrics"/> with the given sinks.
		/// </summary>
		/// <param name="sinks">Sinks to which metrics should be sent</param>
		public SyncMetrics(IEnumerable<ISyncMetricsSink> sinks)
		{
			_sinks = sinks;
		}

		/// <inheritdoc />
		public void TimedOperation(string name, TimeSpan duration, CommandExecutionStatus executionStatus)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.TimedOperation(name, duration, executionStatus);
				sink.Log(metric);
			}
		}
	}
}
