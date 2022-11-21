using System.Net;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter.DNS
{
    public class RealDnsService : IDns
    {
        public IPHostEntry GetHostEntry(string hostName)
        {
            return Dns.GetHostEntry(hostName);
        }
    }
}
