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
		/// <param name="name">Name of the metric.</param>
		/// <param name="customData">Data associated with the metric, namely value and metadata.</param>
		void Count(string name, Dictionary<string, object> customData);


		/// <summary>
		///     Logs a single metric with the given data as gauge operation.
		/// </summary>
		/// <param name="name">Name of the metric.</param>
		/// <param name="correlationId">Correlation id used to group batches of a single job together</param>
		/// <param name="customData">Data associated with the metric, namely value and metadata.</param>
		void Gauge(string name, string correlationId, Dictionary<string, object> customData);
	}
}
