using System.Diagnostics;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public static class ProcessMemoryHelper
    {
        private static Process _CurrentProcess;

        public static long GetCurrentProcessMemoryUsage()
        {
            _CurrentProcess = Process.GetCurrentProcess();
            return _CurrentProcess.PrivateMemorySize64;
        }
    }
}
