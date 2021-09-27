using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common.Metrics;
using Relativity.API;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
	public class MemoryUsageReporter : IMemoryUsageReporter
	{
		private IAPM _apmClient;
		private IAPILog _logger;
		private IRipMetrics _ripMetric;
		private IScheduler _timerScheduler;
		private IProcessMemoryHelper _processMemoryHelper;

		private static string _METRIC_LOG_NAME = "Relativity.IntegrationPoints.Performance.System";
		private static string _METRIC_NAME = "IntegrationPoints.Performance.System";


		public MemoryUsageReporter(IAPM apmClient, IAPILog logger, IRipMetrics ripMetric, IProcessMemoryHelper processMemoryHelper ,IScheduler scheduler = null)
		{
			_timerScheduler = scheduler ?? Scheduler.Default;
			_processMemoryHelper = processMemoryHelper;
			_apmClient = apmClient;
			_logger = logger;
			_ripMetric = ripMetric;
		}

		public IDisposable ActivateTimer(int timeIntervalInSeconds, long jobId, string jobDetails, string jobType)
		{
			return Observable.Timer(dueTime: TimeSpan.Zero, period: TimeSpan.FromSeconds(timeIntervalInSeconds), scheduler: _timerScheduler)
				.Do(x => Execute(jobId, jobDetails, jobType))
				.Subscribe();
		}

		private void Execute(long jobId, string workflowId, string jobType)
		{
            try
            {
				long memoryUsage = _processMemoryHelper.GetCurrentProcessMemoryUsage();

				Dictionary<string, object> customData = new Dictionary<string, object>()
				{
					{ "MemoryUsage", memoryUsage },
					{ "JobId", jobId },
					{ "JobType", jobType },
					{ "WorkflowId", workflowId}
				};

				_processMemoryHelper.GetApplicationSystemStats().ToList().ForEach(x => customData.Add(x.Key, x.Value));

				_apmClient.CountOperation(_METRIC_NAME, correlationID: workflowId, customData: customData).Write();
				_logger.LogInformation("Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {@CorrelationId}", _METRIC_LOG_NAME, customData, _ripMetric.GetWorkflowId());
			}
			catch (Exception ex)
            {
				_logger.LogError(ex, "An error occured in Execute while sending APM metric");
            }
		}
	}
}
