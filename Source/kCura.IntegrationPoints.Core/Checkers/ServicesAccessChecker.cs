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

        public async Task<bool> CheckDatabaseAccessAsync()
        {
            WorkspaceDBContext workspaceDbContext = new WorkspaceDBContext(_helper.GetDBContext(-1));
            DatabasePingReporter dbPingReporter = new DatabasePingReporter(workspaceDbContext, _logger);
            Dictionary<string, object> statistics = await dbPingReporter.GetStatisticAsync().ConfigureAwait(false);
            if (statistics.Count > 0)
            {
                return (bool)statistics["IsDatabaseAccessible"];
            }

            return false;
        }

        public async Task<bool> CheckKeplerAccessAsync()
        {
            KeplerPingReporter keplerPingReporter = new KeplerPingReporter(_helper, _logger);
            Dictionary<string, object> statistics = await keplerPingReporter.GetStatisticAsync().ConfigureAwait(false);
            if (statistics.Count > 0)
            {
                return (bool)statistics["IsKeplerServiceAccessible"];
            }

            return false;
        }

        public async Task<bool> GetFileShareDiskUsageReporterAsync()
        {
            FileShareDiskUsageReporter fileShareDiskUsageReporter = new FileShareDiskUsageReporter(_helper, _logger);
            Dictionary<string, object> statistic = await fileShareDiskUsageReporter.GetStatisticAsync().ConfigureAwait(false);

            // If fileshare is healthy then it is readable and it will we be added to statistics. One is enough.
            bool isAnyFileShareHealthy = statistic.Count > 0;
            return isAnyFileShareHealthy;
        }
    }
}
