using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class CheckAllSyncWorkerBatchesAreFinished : IQuery<bool>
	{
		private readonly IQueueDBContext _dbContext;

		private readonly long _rootJobId;

		public CheckAllSyncWorkerBatchesAreFinished(IQueueDBContext dbContext, long rootJobId)
		{
			_dbContext = dbContext;
			_rootJobId = rootJobId;
		}

		public bool Execute()
		{
			string sql = string.Format(Resources.CheckAllSyncWorkerBatchesAreFinished, _dbContext.TableName);

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@RootJobID", _rootJobId)
			};

			return _dbContext.EddsDBContext.ExecuteSqlStatementAsScalar<bool>(sql, sqlParams.ToArray());
		}
	}
}
