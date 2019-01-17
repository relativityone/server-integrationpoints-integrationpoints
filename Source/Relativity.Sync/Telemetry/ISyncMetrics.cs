using System;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	/// Provides methods for logging metrics.
	/// </summary>
	public interface ISyncMetrics
	{
		/// <summary>
		/// Logs a single execution time along with execution status.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		/// <param name="duration">Execution duration.</param>
		/// <param name="executionStatus">Execution status.</param>
		void TimedOperation(string name, TimeSpan duration, string executionStatus);
	}
}