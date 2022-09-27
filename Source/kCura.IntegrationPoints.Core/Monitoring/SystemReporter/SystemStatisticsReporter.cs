using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Extensions;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public class SystemStatisticsReporter : IHealthStatisticReporter
    {
        private readonly IAPILog _logger;

        public SystemStatisticsReporter(IAPILog logger)
        {
            _logger = logger;
        }

        private PerformanceCounter _cpuUsageSystemTotal { get; } = new PerformanceCounter("Processor", "% Processor Time", "_Total");

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
                FreePhysicalMemory = double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = double.Parse(mo["TotalVisibleMemorySize"].ToString())
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
                List<DriveInfo> physicalDriveInfos = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed).ToList();
                _logger.LogDebug("Found {drivesCount} physical disc(s).", physicalDriveInfos.Count);
                foreach (DriveInfo drive in physicalDriveInfos)
                {
                    try
                    {
                        _logger.LogDebug("Checking drive: {driveName}", drive.Name);
                        if (drive.IsReady)
                        {
                            double freeSpace = drive.TotalFreeSpace;
                            double totalSize = drive.TotalSize;
                            physicalDrivesStatistics.Add($"SystemDisc_{drive.Name}_UsagePercentage", Math.Round(100 * freeSpace / totalSize, 2));
                            physicalDrivesStatistics.Add($"SystemDisc_{drive.Name}_FreeSpaceGB", freeSpace.ConvertBytesToGigabytes());
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(exception, "Cannot check physical drive {driveName}", drive.Name);
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
