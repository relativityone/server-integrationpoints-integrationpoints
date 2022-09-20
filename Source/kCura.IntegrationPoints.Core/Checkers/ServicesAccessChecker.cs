using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Checkers
{
    public class ServicesAccessChecker
    {
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        public ServicesAccessChecker(IHelper helper, IAPILog logger)
        {
            _helper = helper;
            _logger = logger;
        }

        public async Task<bool> AreServicesHealthy()
        {
            ServicesAccessChecker servicesAccessChecker = new ServicesAccessChecker(_helper, _logger);

            bool areServicesAccessible = await servicesAccessChecker.CheckDatabaseAccessAsync().ConfigureAwait(false);
            areServicesAccessible &= await servicesAccessChecker.CheckKeplerAccessAsync().ConfigureAwait(false);
            areServicesAccessible &= await servicesAccessChecker.GetFileShareDiskUsageReporterAsync().ConfigureAwait(false);
            return areServicesAccessible;
        }

        private async Task<bool> CheckDatabaseAccessAsync()
        {
            WorkspaceDBContext workspaceDbContext = new WorkspaceDBContext(_helper.GetDBContext(-1));
            DatabasePingReporter dbPingReporter = new DatabasePingReporter(workspaceDbContext, _logger);
            Dictionary<string, object> statistics = await dbPingReporter.GetStatisticAsync().ConfigureAwait(false);
            if (statistics.Count > 0)
            {
                return (bool)statistics["IsDatabaseAccessible"];
            }

            _logger.LogError("Database is not accessible.");
            return false;
        }

        private async Task<bool> CheckKeplerAccessAsync()
        {
            KeplerPingReporter keplerPingReporter = new KeplerPingReporter(_helper, _logger);
            Dictionary<string, object> statistics = await keplerPingReporter.GetStatisticAsync().ConfigureAwait(false);
            if (statistics.Count > 0)
            {
                return (bool)statistics["IsKeplerServiceAccessible"];
            }

            _logger.LogError("Kepler service is not accessible.");
            return false;
        }

        private async Task<bool> GetFileShareDiskUsageReporterAsync()
        {
            FileShareDiskUsageReporter fileShareDiskUsageReporter = new FileShareDiskUsageReporter(_helper, _logger);
            bool areFileSharesHealthy = await fileShareDiskUsageReporter.CheckFileSharesHealthAsync().ConfigureAwait(false);

            if (!areFileSharesHealthy)
            {
                _logger.LogError("Not all fileShares are healthy.");
            }

            return areFileSharesHealthy;
        }
    }
}
