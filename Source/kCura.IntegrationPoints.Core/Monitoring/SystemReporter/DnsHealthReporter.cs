using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter.DNS;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public class DnsHealthReporter : IServiceHealthChecker
    {
        private static readonly string[] _hostNames = new[]
        {
            "google.com",
            "microsoft.com",
            "amazon.com"
        };

        private readonly IDns _dns;
        private readonly IAPILog _logger;

        public DnsHealthReporter(IDns dns, IAPILog logger)
        {
            _dns = dns;
            _logger = logger.ForContext<DnsHealthReporter>();
        }

        public Task<bool> IsServiceHealthyAsync()
        {
            foreach (string hostName in _hostNames)
            {
                try
                {
                    _dns.GetHostEntry(hostName);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "DNS health check failed because couldn't resolve hostname: {hostName}", hostName);
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }
    }
}
