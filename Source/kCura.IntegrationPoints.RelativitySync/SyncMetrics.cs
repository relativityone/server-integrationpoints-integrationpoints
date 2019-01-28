using System;
using System.Collections.Generic;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Telemetry;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class SyncMetrics : ISyncMetrics
	{
		internal const string RELATIVITY_SYNC_APM_METRIC_NAME = "RelativitySync.Performance.Job";
		internal const string JOB_RESULT_KEY_NAME = "JobResult";

		private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
		private readonly IAPM _apm;
		private readonly IAPILog _logger;
		private DateTime _startTime = DateTime.MinValue;
		private TimeSpan _elapsed = TimeSpan.Zero;

		public SyncMetrics(IAPM apm, IAPILog logger)
		{
			_apm = apm;
			_logger = logger;
		}

		public void TimedOperation(string name, TimeSpan duration, CommandExecutionStatus executionStatus)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Name cannot be empty", nameof(name));
			}
			_data[name] = new StepResult(executionStatus, duration);
			_elapsed += duration;
		}

		public void MarkStartTime()
		{
			_startTime = DateTime.UtcNow;
		}

		public void SendMetric(Guid correlationId, TaskResult taskResult)
		{
			try
			{
				var totalElapsedTimeMs = (_startTime - DateTime.UtcNow).TotalMilliseconds;
				if (taskResult != null)
				{
					_data[JOB_RESULT_KEY_NAME] = taskResult;
					_data["TotalElapsedTimeMs"] = totalElapsedTimeMs;
					_data["AllStepsElapsedTimeMs"] = _elapsed.TotalMilliseconds;
				}

				_apm.TimedOperation(RELATIVITY_SYNC_APM_METRIC_NAME, totalElapsedTimeMs, correlationID: correlationId.ToString(), customData: _data);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Could not send metrics data for job with correlation id {CorrelationId}.", correlationId);
			}
		}
	}
}
