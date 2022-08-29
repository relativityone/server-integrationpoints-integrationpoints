using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public class ProcessMemoryHelper : IProcessMemoryHelper
    {
        private static Process currentProcess { get; set; } = Process.GetCurrentProcess();

        private PerformanceCounter _cpuUsageProcess { get; set; } = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName);

        public Dictionary<string, object> GetApplicationSystemStatistics()
        {
            int megaDivider = (1024 * 1024);
            long memoryProcess = AppDomain.MonitoringSurvivedProcessMemorySize / megaDivider;
            long appDomainCurrentMemory = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize / megaDivider;
            long appDomainLifetimeTotalAllocatedMemory = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize / megaDivider;
            long memoryUsage = GetCurrentProcessMemoryUsage() / megaDivider;
            float cpuUsageProcess = _cpuUsageProcess.NextValue() / Environment.ProcessorCount;

            Dictionary<string, object> appSystemStats = new Dictionary<string, object>()
                {
                    { "SystemProcessMemoryInMB", memoryProcess },
                    { "AppDomainMemoryInMB", appDomainCurrentMemory },
                    { "AppDomainLifetimeTotalAllocatedMemoryInMB", appDomainLifetimeTotalAllocatedMemory },
                    { "PrivateMemoryInMB", memoryUsage },
                    { "CpuUsageProcess",  cpuUsageProcess}
                };
            return appSystemStats;
        }

        private long GetCurrentProcessMemoryUsage()
        {
            return currentProcess.PrivateMemorySize64;
        }

    }
}
