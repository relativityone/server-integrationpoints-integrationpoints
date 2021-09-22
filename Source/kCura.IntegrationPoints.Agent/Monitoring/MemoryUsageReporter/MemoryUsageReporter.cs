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
		private string _workflowId;
		private static string _METRIC_LOG_NAME = "Relativity.IntegrationPoints.Performance.System";
		private static string _METRIC_NAME = "IntegrationPoints.Performance.System";


		public MemoryUsageReporter(IAPM apmClient, IAPILog logger, IRipMetrics ripMetric)
		{
			_apmClient = apmClient;
			_logger = logger;
			_ripMetric = ripMetric;
			_timerThread = new Timer(state => Execute(), null, Timeout.Infinite, Timeout.Infinite);
		}

		public IDisposable ActivateTimer(int timeInterval, long jobId, string jobDetails, string jobType)
		{
			SetJobData(jobId, jobType, jobDetails);
			_timerThread.Change(0, timeInterval);
			return _timerThread;
		}

		private void SetJobData(long jobId, string jobType, string jobDetails)
        {
			_jobId = jobId;
			_jobType = jobType;
			_workflowId = jobDetails;
        }

		private void Execute()
		{
			long memoryUsage = ProcessMemoryHelper.GetCurrentProcessMemoryUsage();

			Dictionary<string, object> customData = new Dictionary<string, object>()
			{
				{ "MemoryUsage", memoryUsage },
				{ "JobId", _jobId },
				{ "JobType", _jobType },
				{ "WorkflowId", _workflowId}
			};

			ProcessMemoryHelper.LogApplicationSystemStats().ToList().ForEach(x => customData.Add(x.Key, x.Value));

			_logger.LogInformation("Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {@CorrelationId}", _METRIC_LOG_NAME, customData, _ripMetric.GetWorkflowId());
			_apmClient.CountOperation(_METRIC_NAME, correlationID: _workflowId, customData: customData).Write();
		}
	}
}
