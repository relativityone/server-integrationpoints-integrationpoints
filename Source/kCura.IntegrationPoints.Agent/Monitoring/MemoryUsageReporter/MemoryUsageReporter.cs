using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using kCura.IntegrationPoints.Common.Metrics;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
	public class MemoryUsageReporter : IMemoryUsageReporter
	{
		private IAPM _apmClient;
		private IAPILog _logger;
		private Timer _timerThread;
		private long _jobId;
		private string _jobType;
		private IRipMetrics _ripMetric;


		public MemoryUsageReporter(IAPM apmClient, IAPILog logger, IRipMetrics ripMetric)
		{
			_apmClient = apmClient;
			_logger = logger;
			_ripMetric = ripMetric;
			_timerThread = new Timer(state => Execute(), null, Timeout.Infinite, Timeout.Infinite);
		}

		public IDisposable ActivateTimer(int timeInterval, long jobId, string jobType)
		{

			SetJobData(jobId, jobType);
			_timerThread.Change(0, timeInterval);
			return _timerThread;
		}

		private void SetJobData(long jobId, string jobType)
        {
			_jobId = jobId;
			_jobType = jobType;
        }

		private void Execute()
		{
			long memoryUsage = ProcessMemoryHelper.GetCurrentProcessMemoryUsage();

			Dictionary<string, object> customData = new Dictionary<string, object>()
				{
					{ "MemoryUsage", memoryUsage },
					{ "JobId", _jobId },
					{ "JobType", _jobType }
				};

			ProcessMemoryHelper.LogApplicationSystemStats().ToList().ForEach(x => customData.Add(x.Key, x.Value));

			_logger.LogInformation("Sending metric \"Relativity.IntegrationPoints.Performance.System\" with properties: {@MetricProperties} and correlationID: {@CorrelationId}", customData.ToString(), _ripMetric.GetWorkflowId());
			_apmClient.CountOperation("IntegrationPoints.Performance.System", correlationID: _ripMetric.GetWorkflowId(), customData: customData).Write();
		}
	}
}
