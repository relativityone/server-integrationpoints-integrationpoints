using System;

namespace kCura.IntegrationPoints.Data.Logging
{
    public interface ISystemEventLoggingService
    {
        void WriteErrorEvent(string source, string logName, Exception ex);
    }
}