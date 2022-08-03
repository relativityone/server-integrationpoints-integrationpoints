using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class UnlockScheduledJob : ICommand
    {
        private readonly IQueueDBContext _dbContext;

        private readonly int _agentId;

        public UnlockScheduledJob(IQueueDBContext dbContext, int agentId)
        {
            _dbContext = dbContext;

            _agentId = agentId;
        }

        public void Execute()
        {
            string sql = string.Format(Resources.UnlockScheduledJob, _dbContext.TableName);

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@AgentID", _agentId));

            _dbContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
        }
    }
}