using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class GetNextJob
	{
		private readonly IQueueDBContext qDBContext = null;
		public GetNextJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public DataTable Execute(int agentID, int agentTypeID, int[] resourceGroupArtifactID)
		{
			string sql = string.Format(Resources.GetNextJob, qDBContext.TableName);
			string ResourceGroupArtifactIDs = string.Join(",", resourceGroupArtifactID);
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
			sqlParams.Add(new SqlParameter("@AgentID", agentID));
			sqlParams.Add(new SqlParameter("@AgentTypeID", agentTypeID));

			return qDBContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
		}
	}
}
