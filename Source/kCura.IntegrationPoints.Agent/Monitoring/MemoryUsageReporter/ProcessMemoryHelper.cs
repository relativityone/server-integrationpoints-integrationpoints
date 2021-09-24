using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public class ProcessMemoryHelper: IProcessMemoryHelper
    {
        public long GetCurrentProcessMemoryUsage()
        {
            return Process.GetCurrentProcess().PrivateMemorySize64;
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
