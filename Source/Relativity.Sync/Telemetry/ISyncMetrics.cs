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
	}
}