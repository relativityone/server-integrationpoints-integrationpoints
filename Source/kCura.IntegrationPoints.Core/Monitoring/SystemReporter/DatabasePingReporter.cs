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
            bool sqlStatementResult = false;
            const int queryValue = 1;
            string sql = $"Select {queryValue}";
            try
            {
                _logger.LogInformation("Checking access to SQL");

                DataTable result = _context.ExecuteSqlStatementAsDataTable(sql);
                sqlStatementResult = result.Columns.Count.Equals(queryValue);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Cannot check Database Service Status.");
            }

            return Task.FromResult(sqlStatementResult);
        }
    }
}
