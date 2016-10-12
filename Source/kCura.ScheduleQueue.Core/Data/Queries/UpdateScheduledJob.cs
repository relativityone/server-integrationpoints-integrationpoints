using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
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
			string sql = string.Format(Resources.UpdateScheduledJob, qDBContext.TableName);

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));
			sqlParams.Add(new SqlParameter("@NextRunTime", nextUTCRunTime));

			qDBContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
