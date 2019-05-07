using System;
using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	/// Provides methods for logging metrics.
	/// </summary>
	internal interface ISyncMetrics
	{
		/// <summary>
		/// Logs a single execution time along with execution status.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		/// <param name="duration">Execution duration.</param>
		/// <param name="executionStatus">Execution status.</param>
		void TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus);

		/// <summary>
		/// Logs a single execution time along with execution status and custom data.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		/// <param name="duration">Execution duration.</param>
		/// <param name="executionStatus">Execution status.</param>
		/// <param name="customData">Custom data.</param>
		void TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus, Dictionary<string, object> customData);

		/// <summary>
		/// Logs a single count operation along with execution status.
		/// </summary>
		/// <param name="name">Name of the counter.</param>
		/// <param name="status">Execution status of the command.</param>
		void CountOperation(string name, ExecutionStatus status);

		/// <summary>
		/// Log time elapsed with the given data.
		/// </summary>
		/// <param name="name">Name of the metric.</param>
		/// <param name="executionStatus">Execution status.</param>
		/// <param name="customData">Data associated with the metric, namely value and metadata.</param>
		/// <returns>A disposable object. Wrap this inside a using block so the dispose can be called to stop the timing.</returns>
		IDisposable TimedOperation(string name, ExecutionStatus executionStatus, Dictionary<string, object> customData);

		/// <summary>
		/// Logs a single gauge metric with the given data.
		/// </summary>
		/// <param name="name">Name of the metric.</param>
		/// <param name="executionStatus">Execution status.</param>
		/// <param name="value">A function to return a gauge value to be logged.</param>
		/// <param name="unitOfMeasure">A string value to describe what the metric's Value property is.</param>
		/// <param name="customData">Data associated with the metric, namely value and metadata.</param>
		void GaugeOperation(string name, ExecutionStatus executionStatus, long value, string unitOfMeasure, Dictionary<string, object> customData);
	}
}