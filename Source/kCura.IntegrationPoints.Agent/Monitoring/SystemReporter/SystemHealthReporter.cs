using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using kCura.Utility;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class SystemHealthReporter : ISystemHealthReporter
    {
        private Dictionary<string, object> _systemHealthStatistics = new Dictionary<string, object>();
        private readonly IDiskUsageReporter _diskUsageReporter;
        private readonly IKeplerPingReporter _keplerPingReporter;
        private readonly IDatabasePingReporter _databasePingReporter;

        private PerformanceCounter _cpuUsageSystemTotal { get; } = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public SystemHealthReporter(IDiskUsageReporter diskUsageReporter, IKeplerPingReporter keplerPingReporter, IDatabasePingReporter databasePingReporter)
        {
            _diskUsageReporter = diskUsageReporter;
            _keplerPingReporter = keplerPingReporter;
            _databasePingReporter = databasePingReporter;
        }

        public Dictionary<string, object> GetSystemHealthStatistics()
        {
            AddToSystemHealthDictionary(_diskUsageReporter.GetFileShareUsage());
            AddToSystemHealthDictionary(GetSystemDiscStatistics());
            AddToSystemHealthDictionary(GetSystemCpuStatistics());
            AddToSystemHealthDictionary(GetSystemMemoryStatistics());
            AddToSystemHealthDictionary(GetKeplerServiceStatus());
            AddToSystemHealthDictionary(GetDatabaseStatus());
            return _systemHealthStatistics;
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

        private void AddToSystemHealthDictionary(Dictionary<string, object> additionalDictionary)
        {
            additionalDictionary.ToList().ForEach(f => _systemHealthStatistics.Add(f.Key, f.Value));
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
            DriveSpace systemDiscDrive = new DriveSpace("C");
            double systemDiscFreePercent = systemDiscDrive.FreePercent;
            long systemDiscFreeSpaceGB = systemDiscDrive.UsedSpace;
            return new Dictionary<string, object>()
            {
                { "SystemDiscCUsage", systemDiscFreePercent },
                { "SystemDiscCFreeSpaceGB", systemDiscFreeSpaceGB }
            };
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
