using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class UnlockJob
	{
		private readonly IQueueDBContext _dbContext;

		public UnlockJob(IQueueDBContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void Execute(long jobID)
		{
			string sql = string.Format(Resources.UnlockJob, _dbContext.TableName);

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@JobID", jobID)
			};

			_dbContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}