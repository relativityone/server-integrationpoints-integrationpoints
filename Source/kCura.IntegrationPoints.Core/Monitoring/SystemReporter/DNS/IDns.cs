using System.Net;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter.DNS
{
    public interface IDns
    {
        IPHostEntry GetHostEntry(string hostName);
    }
}