using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Checkers
{
    public class ServicesAccessChecker
    {
        private readonly IAPILog _logger;
        private readonly IIsServiceHealthy _databasePingReporter;
        private readonly IIsServiceHealthy _keplerPingReporter;
        private readonly IIsServiceHealthy _fileShareDiskUsageReporter;

        public ServicesAccessChecker(
            IIsServiceHealthy databasePingReporter,
            IIsServiceHealthy keplerPingReporter,
            IIsServiceHealthy fileShareDiskUsageReporter,
            IAPILog logger)
        {
            _databasePingReporter = databasePingReporter;
            _keplerPingReporter = keplerPingReporter;
            _fileShareDiskUsageReporter = fileShareDiskUsageReporter;
            _logger = logger;
        }

        public async Task<bool> AreServicesHealthyAsync()
        {
            bool areServicesAccessible = await CheckDatabaseAccessAsync().ConfigureAwait(false);
            areServicesAccessible &= await CheckKeplerAccessAsync().ConfigureAwait(false);
            areServicesAccessible &= await GetFileShareDiskUsageReporterAsync().ConfigureAwait(false);
            return areServicesAccessible;
        }

        private async Task<bool> CheckDatabaseAccessAsync()
        {
            bool isDatabaseHealthy = await _databasePingReporter.IsServiceHealthyAsync().ConfigureAwait(false);
            if (isDatabaseHealthy)
            {
                return true;
            }

            _logger.LogError("Database is not accessible.");
            return false;
        }

        private async Task<bool> CheckKeplerAccessAsync()
        {
            bool isKeplerServiceHealthy = await _keplerPingReporter.IsServiceHealthyAsync().ConfigureAwait(false);
            if (isKeplerServiceHealthy)
            {
                return true;
            }

            _logger.LogError("Kepler service is not accessible.");
            return false;
        }

        private async Task<bool> GetFileShareDiskUsageReporterAsync()
        {
            bool areFileSharesHealthy = await _fileShareDiskUsageReporter.IsServiceHealthyAsync().ConfigureAwait(false);

            if (!areFileSharesHealthy)
            {
                _logger.LogError("Not all fileShares are healthy.");
            }

            return areFileSharesHealthy;
        }
    }
}
