using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using kCura.IntegrationPoints.Common.Metrics;
using Relativity.API;
using System.Diagnostics;
using System.Management;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
	public class MemoryUsageReporter : IMemoryUsageReporter
	{
		private IAPM _apmClient;
		private IAPILog _logger;
		private IRipMetrics _ripMetric;
		private long _jobId;
		private string _jobType;
		private string _workflowId;

		private static string _METRIC_LOG_NAME = "Relativity.IntegrationPoints.Performance.System";
		private static string _METRIC_NAME = "IntegrationPoints.Performance.System";


		public MemoryUsageReporter(IAPM apmClient, IAPILog logger, IRipMetrics ripMetric)
		{
			_apmClient = apmClient;
			_logger = logger;
			_ripMetric = ripMetric;
		}

		public IDisposable ActivateTimer(int timeInterval, long jobId, string jobDetails, string jobType, IScheduler scheduler = null)
		{
			SetJobData(jobId, jobType, jobDetails);

			var timerScheduler = scheduler ?? Scheduler.Default;

			return Observable.Timer(dueTime: TimeSpan.Zero, period: TimeSpan.FromSeconds(timeInterval), scheduler: timerScheduler)
				.Do(x => Execute())
				.Subscribe();
		}

		private void SetJobData(long jobId, string jobType, string jobDetails)
        {
			_jobId = jobId;
			_jobType = jobType;
			_workflowId = jobDetails;
        }

		private void Execute()
		{
            try
            {
				long memoryUsage = ProcessMemoryHelper.GetCurrentProcessMemoryUsage();

				Dictionary<string, object> customData = new Dictionary<string, object>()
				{
					{ "MemoryUsage", memoryUsage },
					{ "JobId", _jobId },
					{ "JobType", _jobType },
					{ "WorkflowId", _workflowId}
				};

				LogApplicationSystemStats().ToList().ForEach(x => customData.Add(x.Key, x.Value));

				_apmClient.CountOperation(_METRIC_NAME, correlationID: _workflowId, customData: customData).Write();
				_logger.LogInformation("Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {@CorrelationId}", _METRIC_LOG_NAME, customData, _ripMetric.GetWorkflowId());
			}
			catch (Exception ex)
            {
				_logger.LogError(ex, "An error occured in Execute while sending APM metric");
            }
		}

		public Dictionary<string, object> LogApplicationSystemStats()
		{
			var memoryProcess = AppDomain.MonitoringSurvivedProcessMemorySize / (1024 * 1024);
			var AppDomainCurrentMemory = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize / (1024 * 1024);
			var AppDomainLifetimeTotalAllocatedMemory = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize / (1024 * 1024);
			var currentProcess = Process.GetCurrentProcess();
			long memoryUsage = currentProcess.PrivateMemorySize64 / (1024 * 1024);

			Dictionary<string, object> appSystemStats = new Dictionary<string, object>()
				{
					{ "SystemProcessMemoryInMB", memoryProcess },
					{ "AppDomainMemoryInMB", AppDomainCurrentMemory },
					{ "AppDomainLifetimeTotalAllocatedMemoryInMB", AppDomainLifetimeTotalAllocatedMemory },
					{ "PrivateMemoryInMB", memoryUsage },
					{ "SystemFreeMemoryPercent",  LogSystemPerformanceStats()}
				};

			return appSystemStats;

		}

		private double LogSystemPerformanceStats()
		{
			var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

			var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
				FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
				TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
			}).FirstOrDefault();

			if (memoryValues != null)
			{
				var percent = (memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize;

				return percent * 100;
			}

			return 0;
		}
	}
}
