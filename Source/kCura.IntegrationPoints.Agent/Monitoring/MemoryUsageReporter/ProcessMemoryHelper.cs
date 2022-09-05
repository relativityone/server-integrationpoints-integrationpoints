using System;
using System.Collections.Generic;
using System.Diagnostics;
using kCura.IntegrationPoints.Core.Extensions;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public class ProcessMemoryHelper : IProcessMemoryHelper
    {
        private PerformanceCounter _cpuUsageProcess = new PerformanceCounter("Process", "% Processor Time", _currentProcess.ProcessName);
        private static Process _currentProcess = Process.GetCurrentProcess();

        public Dictionary<string, object> GetApplicationSystemStatistics()
        {
            double memoryProcess = MakeItHumanReadable(AppDomain.MonitoringSurvivedProcessMemorySize);
            double appDomainCurrentMemory = MakeItHumanReadable(AppDomain.CurrentDomain.MonitoringSurvivedMemorySize);
            double appDomainLifetimeTotalAllocatedMemory = MakeItHumanReadable(AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize);
            double memoryUsage = MakeItHumanReadable(GetCurrentProcessMemoryUsage());
            double cpuUsageProcess = Math.Round((_cpuUsageProcess.NextValue() / Environment.ProcessorCount), 2);

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
            return _currentProcess.PrivateMemorySize64;
        }

        private double MakeItHumanReadable(double number)
        {
            return number.ConvertBytesToGigabytes();
        }
    }
}
