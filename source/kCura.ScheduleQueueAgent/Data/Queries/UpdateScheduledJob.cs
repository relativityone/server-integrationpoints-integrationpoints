using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueueAgent.Properties;

namespace kCura.ScheduleQueueAgent.Data.Queries
{
	public class UpdateScheduledJob
	{
		private IQueueDBContext qDBContext = null;
		public UpdateScheduledJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute(long jobID, DateTime nextUTCRunTime)
		{
			string sql = string.Format(Resources.UpdateScheduledJob, qDBContext.QueueTable);

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));
			sqlParams.Add(new SqlParameter("@NextRunTime", nextUTCRunTime));

			qDBContext.DBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
