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
		private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
		private readonly IAPM _apm;
		private readonly IAPILog _logger;
		private TimeSpan _elapsed = TimeSpan.Zero;

		public SyncMetrics(IAPM apm, IAPILog logger)
		{
			_apm = apm;
			_logger = logger;
		}

		public void TimedOperation(string name, TimeSpan duration, CommandExecutionStatus executionStatus)
		{
			_data[name] = new StepResult(executionStatus, duration);
			_elapsed += duration;
		}

		public void SendMetric(Guid correlationId, TaskResult taskResult)
		{
			try
			{
				_data.Add("JobResult", taskResult);
				_apm.TimedOperation("RelativitySync.Performance.Job", _elapsed.TotalMilliseconds,
					correlationID: correlationId.ToString(), customData: _data);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Could not send metrics data for job with correlation id {CorrelationId}.", correlationId);
			}
		}
	}
}
