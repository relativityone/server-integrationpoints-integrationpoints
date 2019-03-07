using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Logs metrics to an APM system.
	/// </summary>
	internal interface IAPMClient
	{
		/// <summary>
		///     Logs a single metric with the given data.
		/// </summary>
		/// <param name="name">Name of the metric</param>
		/// <param name="customData">Data associated with the metric, namely value and metadata</param>
		void Log(string name, Dictionary<string, object> customData);
	}
}
