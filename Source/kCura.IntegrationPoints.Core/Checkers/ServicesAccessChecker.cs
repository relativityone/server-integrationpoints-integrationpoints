using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Checkers
{
    public class ServicesAccessChecker : IServicesAccessChecker
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
            return statistics.Count > 0;
        }

        public async Task<bool> CheckKeplerAccessAsync()
        {
            KeplerPingReporter keplerPingReporter = new KeplerPingReporter(_helper, _logger);
            Dictionary<string, object> statistics = await keplerPingReporter.GetStatisticAsync().ConfigureAwait(false);
            return statistics.Count > 0;
        }

        public async Task<bool> GetFileShareDiskUsageReporterAsync()
        {
            FileShareDiskUsageReporter fileShareDiskUsageReporter = new FileShareDiskUsageReporter(_helper, _logger);
            Dictionary<string, object> statistics = await fileShareDiskUsageReporter.GetStatisticAsync().ConfigureAwait(false);
            return statistics.Count > 0;
        }
    }
}