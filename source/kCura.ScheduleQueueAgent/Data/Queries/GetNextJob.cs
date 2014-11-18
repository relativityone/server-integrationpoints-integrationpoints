using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueueAgent.Properties;

namespace kCura.ScheduleQueueAgent.Data.Queries
{
	public class GetNextJob
	{
		private IQueueDBContext qDBContext = null;
		public GetNextJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public DataRow Execute(int agentID, int agentTypeID, int processingStatus, int[] resourceGroupArtifactID)
		{
			string sql = string.Format(Resources.CreateQueueTable, qDBContext.QueueTable);
			string ResourceGroupArtifactIDs = string.Join(",", resourceGroupArtifactID);
			sql = sql.Replace("@ResourceGroupArtifactIDs", ResourceGroupArtifactIDs);

#if TIME_MACHINE
             //TODO: implement Time Machine. see Legal Hold
#endif

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@AgentID", agentID));
			sqlParams.Add(new SqlParameter("@AgentTypeID", agentTypeID));
			sqlParams.Add(new SqlParameter("@ProcessingStatus", processingStatus));

			var dataTable = qDBContext.DBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());

			DataRow row = null;
			if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
				row = dataTable.Rows[0];
			else
				row = null;

			return row;
		}
	}
}
