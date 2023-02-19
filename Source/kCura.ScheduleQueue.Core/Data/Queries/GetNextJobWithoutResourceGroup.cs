using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetNextJobWithoutResourceGroup : IQuery<DataTable>
    {
        private readonly IQueueDBContext _dbContext;
        private readonly int _agentId;
        private readonly int _agentTypeId;
        private readonly long? _rootJobId;

        public GetNextJobWithoutResourceGroup(IQueueDBContext dbContext, int agentId, int agentTypeId, long? rootJobId)
        {
            _dbContext = dbContext;

            _agentId = agentId;
            _agentTypeId = agentTypeId;
            _rootJobId = rootJobId;
        }

        public DataTable Execute()
        {
            string sql = string.Format(Resources.GetNextJobWithoutResourceGroup, _dbContext.TableName);

            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@AgentID", _agentId),
                new SqlParameter("@AgentTypeID", _agentTypeId),
                new SqlParameter("@RootJobID", _rootJobId ?? (object) DBNull.Value)
            };

            DataTable dataTable = _dbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
            return dataTable;
        }
    }
}
