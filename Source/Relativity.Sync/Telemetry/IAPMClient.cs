using System;
using System.Collections.Generic;
using Relativity.Telemetry.APM;

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
		void Log(string name, Dictionary<string, object> customData);

		/// <summary>
		///		Log time elapsed with the given data.
		/// </summary>
		/// <param name="name">Name of the metric.</param>
		/// <param name="correlationId">The correlation identifier. Can be used to associate multiple metrics.</param>
		/// <param name="customData">Data associated with the metric, namely value and metadata.</param>
		/// <returns>A disposable object. Wrap this inside a using block so the dispose can be called to stop the timing.</returns>
		IDisposable TimedOperation(string name, string correlationId, Dictionary<string, object> customData);

		/// <summary>
		///		Logs a single gauge metric with the given data.
		/// </summary>
		/// <typeparam name="T">Gauge value type. It can be one of the following types: [int, long, double]</typeparam>
		/// <param name="name">Name of the metric.</param>
		/// <param name="correlationId">The correlation identifier. Can be used to associate multiple metrics.</param>
		/// <param name="operation">A function to return a gauge value to be logged.</param>
		/// <param name="unitOfMeasure">A string value to describe what the metric's Value property is.</param>
		/// <param name="customData">Data associated with the metric, namely value and metadata.</param>
		void GaugeOperation<T>(string name, string correlationId, Func<T> operation, string unitOfMeasure, Dictionary<string, object> customData);
	}
}
