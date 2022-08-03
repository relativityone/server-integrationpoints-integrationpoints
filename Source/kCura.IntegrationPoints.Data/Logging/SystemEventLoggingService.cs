using System;

namespace kCura.IntegrationPoints.Data.Logging
{
    public class SystemEventLoggingService : ISystemEventLoggingService
    {
        public void WriteErrorEvent(string source, string logName, Exception ex)
        {
            if (!System.Diagnostics.EventLog.SourceExists(source))
            {
                System.Diagnostics.EventLog.CreateEventSource(source, logName);
            }

            System.Diagnostics.EventLog.WriteEntry(source, ex.Message + Environment.NewLine + ex.StackTrace,
                System.Diagnostics.EventLogEntryType.Error);
        }
    }
}
