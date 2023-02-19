using System;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class CleanupJobQueueTable : ICommand
    {
        private const string _RELATIVITY_INTEGRATION_POINTS_AGENT_GUID = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
        private readonly IQueueDBContext _context;

        public CleanupJobQueueTable(IQueueDBContext context)
        {
            _context = context;
        }

        public void Execute()
        {
            var agentGuidParameter = new SqlParameter("@agentGuid", SqlDbType.NVarChar)
            {
                Value = _RELATIVITY_INTEGRATION_POINTS_AGENT_GUID
            };
            var sqlParameters = new[] {agentGuidParameter};

            string sql = string.Format(Resources.CleanupJobQueueTable, _context.TableName);

            _context.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParameters);
        }
    }
}
