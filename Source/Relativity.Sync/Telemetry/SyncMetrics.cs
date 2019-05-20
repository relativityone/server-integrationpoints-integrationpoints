using System;
using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Entry point for logging metrics. Dispatches metrics to registered <see cref="ISyncMetricsSink" />s for processing.
	/// </summary>
	internal sealed class SyncMetrics : ISyncMetrics
	{
		private readonly IEnumerable<ISyncMetricsSink> _sinks;
		private readonly CorrelationId _correlationId;

		/// <summary>
		///     Creates a new instance of <see cref="SyncMetrics" /> with the given sinks.
		/// </summary>
		/// <param name="sinks">Sinks to which metrics should be sent</param>
		/// <param name="correlationId">ID which correlates all metrics across a job</param>
		public SyncMetrics(IEnumerable<ISyncMetricsSink> sinks, CorrelationId correlationId)
		{
			_sinks = sinks;
			_correlationId = correlationId;
		}

		/// <inheritdoc />
		public void TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.TimedOperation(name, duration, executionStatus, _correlationId.Value);
				sink.Log(metric);
			}
		}

		/// <inheritdoc />
		public void TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.TimedOperation(name, duration, executionStatus, _correlationId.Value);
				foreach (KeyValuePair<string, object> keyValuePair in customData)
				{
					metric.CustomData.Add(keyValuePair);
				}

				sink.Log(metric);
			}
		}

		/// <inheritdoc />
		public void CountOperation(string name, ExecutionStatus status)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.CountOperation(name, status, _correlationId.Value);
				sink.Log(metric);
			}
		}

		/// <inheritdoc />
		public IDisposable TimedOperation(string name, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			return new DisposableStopwatch(timeSpan => TimedOperation(name, timeSpan, executionStatus, customData));
		}

		/// <inheritdoc />
		public IDisposable TimedOperation(string name, ExecutionStatus executionStatus)
		{
			return new DisposableStopwatch(timeSpan => TimedOperation(name, timeSpan, executionStatus));
		}

		/// <inheritdoc />
		public void GaugeOperation(string name, ExecutionStatus status, long value, string unitOfMeasure, Dictionary<string, object> customData)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.GaugeOperation(name, status, _correlationId.Value, value, unitOfMeasure);
				foreach (KeyValuePair<string, object> keyValuePair in customData)
				{
					metric.CustomData.Add(keyValuePair);
				}

				sink.Log(metric);
			}
		}
	}
}