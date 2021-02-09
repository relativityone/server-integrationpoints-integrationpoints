﻿namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Sink for logging <see cref="Metric"/>s to a specific collector
	/// </summary>
	internal interface ISyncMetricsSink
	{
		/// <summary>
		///     Handles the given metric.
		/// </summary>
		/// <param name="metric">Metric to log</param>
		void Log(Metric metric);

		/// <summary>
		///     Send metric for given Sink
		/// </summary>
		/// <param name="metric">Metric to send</param>
		void Send(IMetric metric);
	}
}
