using System;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	/// </summary>
	public interface ISyncMetrics
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="duration"></param>
		/// <param name="executionStatus"></param>
		void TimedOperation(string name, TimeSpan duration, string executionStatus);
	}
}