using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetNextJob : IQuery<DataTable>
    {
        private readonly IQueueDBContext _dbContext;

        private readonly int _agentId;
        private readonly int _agentTypeId;
        private readonly int[] _resourceGroupArtifactId;

        public GetNextJob(IQueueDBContext dbContext, int agentId, int agentTypeId, int[] resourceGroupArtifactId)
        {
            _dbContext = dbContext;

            _agentId = agentId;
            _agentTypeId = agentTypeId;
            _resourceGroupArtifactId = resourceGroupArtifactId;
        }

        public DataTable Execute()
        {
            string sql = string.Format(Resources.GetNextJob, _dbContext.TableName);
            string ResourceGroupArtifactIDs = string.Join(",", _resourceGroupArtifactId);
            sql = sql.Replace("@ResourceGroupArtifactIDs", ResourceGroupArtifactIDs);

#if TIME_MACHINE
            if (AgentTimeMachineProvider.Current.Enabled)
            {
                if (AgentTimeMachineProvider.Current.WorkspaceID > 0)
                {
                    sql = sql.Replace("q.[NextRunTime] <= GETUTCDATE()", String.Format("((q.[NextRunTime] <= GETUTCDATE() AND q.[WorkspaceID]<>{0}) OR (q.[NextRunTime] <= CAST('{1}' AS DATETIME) AND q.[WorkspaceID]={0}))", AgentTimeMachineProvider.Current.WorkspaceID, AgentTimeMachineProvider.Current.UtcNow));
                }
                else
                {
                    sql = sql.Replace("GETUTCDATE()", String.Format("CAST('{0}' AS DATETIME)", AgentTimeMachineProvider.Current.UtcNow));
                }
            }
#endif

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@AgentID", _agentId));
            sqlParams.Add(new SqlParameter("@AgentTypeID", _agentTypeId));

            return _dbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
        }
    }
}
