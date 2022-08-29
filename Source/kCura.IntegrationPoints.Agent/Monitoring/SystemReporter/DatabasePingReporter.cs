using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class DatabasePingReporter : IDatabasePingReporter
    {
        private readonly IWorkspaceDBContext _context;

        public DatabasePingReporter(IWorkspaceDBContext context)
        {
            _context = context;
        }

        public bool IsDatabaseAccessible()
        {
            string sql = "Select 1";
            int result = _context.ExecuteNonQuerySQLStatement(sql);

            return result == 1;
        }
    }
}
