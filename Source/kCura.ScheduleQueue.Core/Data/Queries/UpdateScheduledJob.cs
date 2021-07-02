using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class UpdateScheduledJob : ICommand
	{
		private readonly IQueueDBContext _dbContext;
		
		private readonly long _jobId;
		private readonly DateTime _nextUtcRunTime;

		public UpdateScheduledJob(IQueueDBContext dbContext, long jobId, DateTime nextUtcRunTime)
		{
			_dbContext = dbContext;
			
			_jobId = jobId;
			_nextUtcRunTime = nextUtcRunTime;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.UpdateScheduledJob, _dbContext.TableName);

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", _jobId));
			sqlParams.Add(new SqlParameter("@NextRunTime", _nextUtcRunTime));

			_dbContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
