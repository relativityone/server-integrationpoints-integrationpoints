using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class DeleteJob
	{
		private IQueueDBContext qDBContext;

		public DeleteJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute(long jobID)
		{
			string sql = string.Format(Resources.DeleteJob, qDBContext.TableName);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));

			qDBContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
