using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class DeleteJob
	{
		private readonly IDBContext _dbContext;
		private readonly string _tableName;

		public DeleteJob(IQueueDBContext qDBContext)
		{
			_dbContext = qDBContext.EddsDBContext;
			_tableName = qDBContext.TableName;
		}

		public DeleteJob(IDBContext dbContext, string tableName)
		{
			_dbContext = dbContext;
			_tableName = tableName;
		}

		public void Execute(long jobID)
		{
			string sql = string.Format(Resources.DeleteJob, _tableName);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));

			_dbContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
