using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Checkers
{
    public class ServicesAccessChecker
    {
        private readonly IEnumerable<IServiceHealthChecker> _healthCheckers;
        private readonly IAPILog _logger;

        public ServicesAccessChecker(IEnumerable<IServiceHealthChecker> healthCheckers, IAPILog logger)
        {
            _healthCheckers = healthCheckers;
            _logger = logger;
        }

        public async Task<bool> AreServicesHealthyAsync()
        {
            bool allServicesHealthy;

            try
            {
                List<Task<bool>> tasks = new List<Task<bool>>();

                foreach (IServiceHealthChecker healthChecker in _healthCheckers)
                {
                    Task<bool> task = Task.Run(healthChecker.IsServiceHealthyAsync);
                    tasks.Add(task);
                }

                bool[] results = await Task.WhenAll(tasks).ConfigureAwait(false);

                allServicesHealthy = results.All(isHealthy => isHealthy);

                if (!allServicesHealthy)
                {
                    _logger.LogError("Not all services are healthy.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform health checks");
                allServicesHealthy = false;
            }

            return allServicesHealthy;
        }
    }
}
