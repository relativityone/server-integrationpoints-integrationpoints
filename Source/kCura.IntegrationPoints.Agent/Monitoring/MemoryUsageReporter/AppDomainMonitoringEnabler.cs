using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public class AppDomainMonitoringEnabler : IAppDomainMonitoringEnabler
    {
        private IAPILog _logger;

        public AppDomainMonitoringEnabler(IAPILog logger)
        {
            _logger = logger;
        }

        public bool EnableMonitoring()
        {
            try
            {
                AppDomain.MonitoringIsEnabled = true;
            }
            catch (Exception e)
            {
                _logger.LogError("Could not enable App domain Resource Monitoring for performance metrics", e);
            }

            return AppDomain.MonitoringIsEnabled;
        }
    }
}
