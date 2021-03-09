﻿using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Data.Interfaces;
using kCura.ScheduleQueue.Core.Properties;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class DeleteJob : ICommand
	{
		private readonly IDBContext _dbContext;
		
		private readonly string _tableName;
		private readonly long _jobId;

		public DeleteJob(IQueueDBContext dbContext, long jobId)
		{
			_dbContext = dbContext.EddsDBContext;
			
			_tableName = dbContext.TableName;
			_jobId = jobId;
		}

		public DeleteJob(IDBContext dbContext, string tableName, long jobId)
		{
			_dbContext = dbContext;
			_tableName = tableName;
			
			_jobId = jobId;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.DeleteJob, _tableName);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", _jobId));

			_dbContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
