using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueueAgent.Properties;

namespace kCura.ScheduleQueueAgent.Data.Queries
{
	public class DeleteJob
	{
		private IQueueDBContext qDBContext = null;

		public DeleteJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute(long jobID)
		{
			string sql = string.Format(Resources.DeleteJob, qDBContext.QueueTable);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));

			qDBContext.DBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
