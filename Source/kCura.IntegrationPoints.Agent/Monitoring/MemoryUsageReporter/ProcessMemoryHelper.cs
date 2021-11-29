using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public class ProcessMemoryHelper: IProcessMemoryHelper
    {
		internal PerformanceCounter _cpuUsageProcess { get; set; } = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName);
		internal PerformanceCounter _cpuUsageSystemTotal { get; set; } = new PerformanceCounter("Processor", "% Processor Time", "_Total");
		internal static Process currentProcess { get; set; } = Process.GetCurrentProcess();
		

		public Dictionary<string, object> GetApplicationSystemStats()
		{
			var memoryProcess = AppDomain.MonitoringSurvivedProcessMemorySize / (1024 * 1024);
			var AppDomainCurrentMemory = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize / (1024 * 1024);
			var AppDomainLifetimeTotalAllocatedMemory = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize / (1024 * 1024);
			long memoryUsage = GetCurrentProcessMemoryUsage() / (1024 * 1024);
			float cpuUsageSystem = _cpuUsageSystemTotal.NextValue();
			float cpuUsageProcess = _cpuUsageProcess.NextValue() / Environment.ProcessorCount;

			Dictionary<string, object> appSystemStats = new Dictionary<string, object>()
				{
					{ "SystemProcessMemoryInMB", memoryProcess },
					{ "AppDomainMemoryInMB", AppDomainCurrentMemory },
					{ "AppDomainLifetimeTotalAllocatedMemoryInMB", AppDomainLifetimeTotalAllocatedMemory },
					{ "PrivateMemoryInMB", memoryUsage },
					{ "SystemFreeMemoryPercentage",  GetSystemFreeMemoryPercentage()},
					{ "CpuUsageSystem", cpuUsageSystem },
					{ "CpuUsageProcess",  cpuUsageProcess}
				};

			return appSystemStats;

		}
		private long GetCurrentProcessMemoryUsage()
		{
			return currentProcess.PrivateMemorySize64;
		}

		private double GetSystemFreeMemoryPercentage()
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
