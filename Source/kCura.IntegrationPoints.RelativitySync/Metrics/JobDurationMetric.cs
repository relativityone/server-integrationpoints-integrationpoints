using Relativity.API;
using Relativity.Telemetry.APM;
using Relativity.Telemetry.Services.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
	class JobDurationMetric
	{
//		internal const string RELATIVITY_SYNC_APM_METRIC_NAME = "RelativitySync.Performance.Job";
//		internal const string JOB_RESULT_KEY_NAME = "JobResult";
//		internal const string TOTAL_ELAPSED_TIME_MS = "TotalElapsedTimeMs";
//		internal const string ALL_STEPS_ELAPSED_TIME_MS = "AllStepsElapsedTimeMs";

//		private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
//		private readonly IAPM _apm;
//		private readonly IAPILog _logger;
//		private DateTime _startTime = DateTime.MinValue;
//		private readonly TimeSpan _elapsed = TimeSpan.Zero;

//		public bool CanSend() => true;

//		public void Dispose()
//		{
//			throw new NotImplementedException();
//		}

//		public Task SendAsync()
//		{
//			throw new NotImplementedException();
//		}

//		public JobDurationMetric(IAPM apm, IAPILog logger)
//		{
//			_apm = apm;
//			_logger = logger;

//			_startTime = DateTime.Now;
//		}

//		public void SendMetric(Guid correlationId, TaskResult taskResult)
//		{
//			try
//			{
//				var totalElapsedTimeMs = (DateTime.Now - _startTime).TotalMilliseconds;
//				if (taskResult != null)
//				{
//					_data[JOB_RESULT_KEY_NAME] = taskResult.Status.ToString();
//					_data[TOTAL_ELAPSED_TIME_MS] = totalElapsedTimeMs;
//					_data[ALL_STEPS_ELAPSED_TIME_MS] = _elapsed.TotalMilliseconds;

//					_apm.TimedOperation(RELATIVITY_SYNC_APM_METRIC_NAME, totalElapsedTimeMs, correlationID: correlationId.ToString(), customData: _data);
//				}
//			}
//#pragma warning disable CA1031
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Could not send metrics data for job with correlation id {CorrelationId}.", correlationId);
//			}
//#pragma warning restore CA1031
//		}
	}
}
