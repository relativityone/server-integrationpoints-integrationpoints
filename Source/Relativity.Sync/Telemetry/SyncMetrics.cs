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
		private readonly SyncJobParameters _syncJobParameters;

		/// <summary>
		///     Creates a new instance of <see cref="SyncMetrics" /> with the given sinks.
		/// </summary>
		/// <param name="sinks">Sinks to which metrics should be sent</param>
		/// <param name="syncJobParameters">ID which correlates all metrics across a job</param>
		public SyncMetrics(IEnumerable<ISyncMetricsSink> sinks, SyncJobParameters syncJobParameters)
		{
			_sinks = sinks;
			_syncJobParameters = syncJobParameters;
		}

		/// <inheritdoc />
		public void TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.TimedOperation(name, duration, executionStatus, _syncJobParameters.WorkflowId.Value);
				sink.Log(metric);
			}
		}

		/// <inheritdoc />
		public void TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.TimedOperation(name, duration, executionStatus, _syncJobParameters.WorkflowId.Value);
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
				Metric metric = Metric.CountOperation(name, status, _syncJobParameters.WorkflowId.Value);
				sink.Log(metric);
			}
		}

		/// <inheritdoc />
		public IDisposable TimedOperation(string name, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			return new DisposableStopwatch(timeSpan => TimedOperation(name, timeSpan, executionStatus, customData));
		}

		public IDisposable TimedOperation(string name, ExecutionStatus executionStatus)
		{
			return new DisposableStopwatch(timeSpan => TimedOperation(name, timeSpan, executionStatus));
		}

		/// <inheritdoc />
		public void GaugeOperation(string name, ExecutionStatus executionStatus, long value, string unitOfMeasure, Dictionary<string, object> customData)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.GaugeOperation(name, executionStatus, _syncJobParameters.WorkflowId.Value, value, unitOfMeasure);
				foreach (KeyValuePair<string, object> keyValuePair in customData)
				{
					metric.CustomData.Add(keyValuePair);
				}

				sink.Log(metric);
			}
		}

		public void LogPointInTimeString(string name, string value)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.PointInTimeStringOperation(name, value, _syncJobParameters.WorkflowId.Value);
				sink.Log(metric);
			}
		}

		public void LogPointInTimeLong(string name, long value)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.PointInTimeLongOperation(name, value, _syncJobParameters.WorkflowId.Value);
				sink.Log(metric);
			}
		}

		public void LogPointInTimeDouble(string name, double value)
		{
			foreach (ISyncMetricsSink sink in _sinks)
			{
				Metric metric = Metric.PointInTimeDoubleOperation(name, value, _syncJobParameters.WorkflowId.Value);
				sink.Log(metric);
			}
		}
	}
}