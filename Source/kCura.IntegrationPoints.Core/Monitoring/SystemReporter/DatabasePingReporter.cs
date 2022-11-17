using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public class DatabasePingReporter : IHealthStatisticReporter, IServiceHealthChecker
    {
        private readonly IWorkspaceDBContext _context;
        private readonly IAPILog _logger;

        public DatabasePingReporter(IWorkspaceDBContext context, IAPILog logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> GetStatisticAsync()
        {
            return new Dictionary<string, object>
            {
                { "IsDatabaseAccessible", await IsServiceHealthyAsync().ConfigureAwait(false) }
            };
        }

        public Task<bool> IsServiceHealthyAsync()
        {
            bool sqlStatementResult;

            const int queryValue = 1;
            string sql = $"Select {queryValue}";

            try
            {
                DataTable result = _context.ExecuteSqlStatementAsDataTable(sql);
                sqlStatementResult = result.Columns.Count.Equals(queryValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL health check failed.");
                sqlStatementResult = false;
            }

            return Task.FromResult(sqlStatementResult);
        }
    }
}
