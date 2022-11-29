using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetAgentTypeInformation : IQuery<DataRow>
    {
        private readonly IEddsDBContext DBContext;
        private readonly Guid _agentGuid;

        public GetAgentTypeInformation(IEddsDBContext dbContext, Guid agentGuid)
        {
            this.DBContext = dbContext;
            _agentGuid = agentGuid;
        }

        public DataRow Execute()
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@AgentID", DBNull.Value));
            sqlParams.Add(new SqlParameter("@AgentGuid", _agentGuid));

            return Execute(sqlParams);
        }

        private DataRow Execute(IEnumerable<SqlParameter> sqlParams)
        {
            string sql = Resources.GetAgentTypeInformation;

            var dataTable = DBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);

            DataRow row = null;
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
            {
                row = dataTable.Rows[0];
            }
            else
            {
                row = null;
            }

            return row;
        }
    }
}
