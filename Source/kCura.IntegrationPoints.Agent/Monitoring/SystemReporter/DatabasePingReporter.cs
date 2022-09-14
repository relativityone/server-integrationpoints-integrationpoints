using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class DatabasePingReporter : IHealthStatisticReporter
    {
        private readonly IWorkspaceDBContext _context;
        private readonly IAPILog _logger;

        public DatabasePingReporter(IWorkspaceDBContext context, IAPILog logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<Dictionary<string, object>> GetStatisticAsync()
        {
            return Task.FromResult(new Dictionary<string, object>()
            {
                { "IsDatabaseAccessible", IsDatabaseAccessible() }
            });
        }


        private bool IsDatabaseAccessible()
        {
            bool sqlStatementResult = false;
            const int queryValue = 1;
            string sql = $"Select {queryValue}";
            try
            {
                DataTable result = _context.ExecuteSqlStatementAsDataTable(sql);
                sqlStatementResult = result.Columns.Count.Equals(queryValue);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Cannot check Database Service Status.");
            }

            return sqlStatementResult;
        }
    }
}
