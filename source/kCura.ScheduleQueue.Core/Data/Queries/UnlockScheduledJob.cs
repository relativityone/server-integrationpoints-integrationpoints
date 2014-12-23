using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class UnlockScheduledJob
	{
		private IQueueDBContext qDBContext = null;
		public UnlockScheduledJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute(int agentID)
		{
			string sql = string.Format(Resources.UnlockScheduledJob, qDBContext.TableName);

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@AgentID", agentID));

			qDBContext.DBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
