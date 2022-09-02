using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using kCura.IntegrationPoints.Core.Extensions;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class SystemHealthReporter : ISystemHealthReporter
    {
        private readonly IDiskUsageReporter _diskUsageReporter;
        private readonly IKeplerPingReporter _keplerPingReporter;
        private readonly IDatabasePingReporter _databasePingReporter;
        private readonly IAPILog _logger;

        private PerformanceCounter _cpuUsageSystemTotal { get; } = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public SystemHealthReporter(IDiskUsageReporter diskUsageReporter, IKeplerPingReporter keplerPingReporter, IDatabasePingReporter databasePingReporter, IAPILog logger)
        {
            _diskUsageReporter = diskUsageReporter;
            _keplerPingReporter = keplerPingReporter;
            _databasePingReporter = databasePingReporter;
            _logger = logger;
        }


        public Dictionary<string, object> GetSystemHealthStatistics()
        {
            Dictionary<string, object> systemHealthStatistics = new Dictionary<string, object>();

            systemHealthStatistics
                .AddDictionary(_diskUsageReporter.GetFileShareUsage())
                .AddDictionary(GetSystemDiscStatistics())
                .AddDictionary(GetSystemCpuStatistics())
                .AddDictionary(GetSystemMemoryStatistics())
                .AddDictionary(GetKeplerServiceStatus())
                .AddDictionary(GetDatabaseStatus());

            return systemHealthStatistics;
        }

        private Dictionary<string, object> GetKeplerServiceStatus()
        {
            return new Dictionary<string, object>()
            {
                { "IsKeplerServiceAccessible", _keplerPingReporter.IsKeplerServiceAccessible() }
            };
        }

        private Dictionary<string, object> GetDatabaseStatus()
        {
            return new Dictionary<string, object>()
            {
                { "IsDatabaseAccessible", _databasePingReporter.IsDatabaseAccessible() }
            };
        }

        private Dictionary<string, object> GetSystemMemoryStatistics()
        {
            return new Dictionary<string, object>()
            {
                { "SystemFreeMemoryPercentage",  GetSystemFreeMemoryPercentage() }
            };
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

        private Dictionary<string, object> GetSystemDiscStatistics()
        {
            Dictionary<string, object> physicalDrivesStatistics = new Dictionary<string, object>();
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                double freeSpace = drive.TotalFreeSpace;
                double totalSize = drive.TotalSize;
                physicalDrivesStatistics.Add($"SystemDisc{drive.Name}Usage", 100 * freeSpace / totalSize);
                physicalDrivesStatistics.Add($"SystemDisc{drive.Name}FreeSpaceGB", freeSpace / (1024*1024*1024) );
            }

            return physicalDrivesStatistics;
        }

        private Dictionary<string, object> GetSystemCpuStatistics()
        {
            float cpuUsageSystem = _cpuUsageSystemTotal.NextValue();
            return new Dictionary<string, object>
            {
                { "CpuUsageSystem", cpuUsageSystem }
            };
        }

    }
}
