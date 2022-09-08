using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Extensions;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class SystemStatisticsReporter : IHealthStatisticReporter
    {
        private readonly IAPILog _logger;

        private PerformanceCounter _cpuUsageSystemTotal { get; } = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public SystemStatisticsReporter(IAPILog logger)
        {
            _logger = logger;
        }

        public Task<Dictionary<string, object>> GetStatisticAsync()
        {
            Dictionary<string, object> statistics = new Dictionary<string, object>();
            statistics.Add("SystemFreeMemoryPercentage", GetSystemFreeMemoryPercentage());
            statistics.AddDictionary(GetSystemCpuStatistics());
            statistics.AddDictionary(GetSystemDiscStatistics());
            return Task.FromResult(statistics);
        }

        private double GetSystemFreeMemoryPercentage()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                double percent = (memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) /
                                 memoryValues.TotalVisibleMemorySize;

                return percent * 100;
            }

            return 0;
        }

        private Dictionary<string, object> GetSystemCpuStatistics()
        {
            float cpuUsageSystem = _cpuUsageSystemTotal.NextValue();
            return new Dictionary<string, object>
            {
                { "CpuUsageSystem", cpuUsageSystem }
            };
        }

        private Dictionary<string, object> GetSystemDiscStatistics()
        {
            Dictionary<string, object> physicalDrivesStatistics = new Dictionary<string, object>();
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                _logger.LogInformation($"Found {drives.Length} physical disc(s).");
                foreach (DriveInfo drive in drives)
                {
                    try
                    {
                        _logger.LogInformation($"Checking drive: {drive.Name}");
                        if (drive.IsReady)
                        {
                            double freeSpace = drive.TotalFreeSpace;
                            double totalSize = drive.TotalSize;
                            physicalDrivesStatistics.Add($"SystemDisc{drive.Name}Usage", Math.Round(100 * freeSpace / totalSize, 2));
                            physicalDrivesStatistics.Add($"SystemDisc{drive.Name}FreeSpaceGB", freeSpace.ConvertBytesToGigabytes());
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(exception, $"Cannot check physical drive {drive.Name}");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Cannot check physical drives.");
            }

            return physicalDrivesStatistics;
        }
    }
}
