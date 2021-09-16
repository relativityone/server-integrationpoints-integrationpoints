﻿using Relativity.API;
using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Management;
using System.Linq;
using kCura.IntegrationPoints.Common.Metrics;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
	public class MemoryUsageReporter : IMemoryUsageReporter
	{
		private IAPM _apmClient;
		private Timer _timerThread;
		private long _jobId;
		private string _jobType;
		private RipMetric _ripMetric;


		public MemoryUsageReporter(IAPM apmClient, RipMetric ripMetric)
		{
			_timerThread = new Timer(state => Execute(), null, Timeout.Infinite, Timeout.Infinite);
			_apmClient = apmClient;
			_ripMetric = ripMetric;
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
				{ "Name", "IntegrationPoints.Performance.System" },
				{ "MemoryUsage", memoryUsage },
				{ "JobId", _jobId },
				{ "JobType", _jobType }
			};

			LogApplicationSystemStats().ToList().ForEach(x => customData.Add(x.Key, x.Value));

			_apmClient.CountOperation("Relativity.IntegrationPoints.Performance", customData: customData, correlationID: _ripMetric.WorkflowId).Write();
		}

		private Dictionary<string, object> LogApplicationSystemStats()
		{
			var memoryProcess = AppDomain.MonitoringSurvivedProcessMemorySize / (1024 * 1024);
			var AppDomainCurrentMemory = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize / (1024 * 1024);
			var AppDomainTotalMemory = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize / (1024 * 1024);
			var currentProcess = Process.GetCurrentProcess();
			long memoryUsage = currentProcess.PrivateMemorySize64 / (1024 * 1024);

			Dictionary<string, object> appSystemStats = new Dictionary<string, object>()
			{
				{ "SystemProcessMemory", memoryProcess },
				{ "AppDomainMemory", AppDomainCurrentMemory },
				{ "AppDomainTotalMemory", AppDomainTotalMemory },
				{ "PrivateMemoryBytes", memoryUsage },
				{ "SystemFreeMemory",  LogSystemPerformanceStats()}
			};

			return appSystemStats;
		}

		private double LogSystemPerformanceStats()
		{
			// REMEMBER THAT YOU'VE ADDED System.Management TO THIS PROJECT SO YOU NEED TO CONFIGURE THAT!
			var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

			var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
				FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
				TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
			}).FirstOrDefault();

			if (memoryValues != null)
			{
				var percent = (memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize;

				return percent;
			}

			return 0;
		}
	}
}
