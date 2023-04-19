using Serilog;

namespace kCura.IntegrationPoints.Common.Logger
{
    public interface ISerilogLoggerInstrumentationService
    {
        ILogger GetLogger();
    }
}