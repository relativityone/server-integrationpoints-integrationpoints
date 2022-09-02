using System;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class DatabasePingReporter : IDatabasePingReporter
    {
        private readonly IWorkspaceDBContext _context;
        private readonly IAPILog _logger;

        public DatabasePingReporter(IWorkspaceDBContext context, IAPILog logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool IsDatabaseAccessible()
        {
            bool sqlStatmentResult = false;
            string sql = "Select 1";
            try
            {
                int result = _context.ExecuteNonQuerySQLStatement(sql);
                sqlStatmentResult = result.Equals(1);
            }
            catch (Exception exception)
            {
                _logger.LogWarning($"Cannot check Database Service Status. Exception {exception}");
            }

            return sqlStatmentResult;
        }
    }
}
