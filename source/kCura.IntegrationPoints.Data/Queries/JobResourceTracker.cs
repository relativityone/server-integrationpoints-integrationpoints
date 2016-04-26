using System.Collections.Generic;
using System.Data.SqlClient;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class JobResourceTracker
	{
		private readonly IWorkspaceDBContext _context;
		public JobResourceTracker(IWorkspaceDBContext context)
		{
			_context = context;
		}

		public void CreateTrackingEntry(string tableName, long jobId)
		{
			var sql = Resources.Resource.CreateJobTrackingEntry;
			var sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@tableName", tableName),
				new SqlParameter("@jobID", jobId)
			};
			_context.ExecuteNonQuerySQLStatement(sql, sqlParams);
		}

		public int RemoveEntryAndCheckStatus(string tableName, long jobId)
		{
			var sql = Resources.Resource.RemoveEntryAndCheckBatchStatus;
			var sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@tableName", tableName),
				new SqlParameter("@jobID", jobId)
			};
			return _context.ExecuteSqlStatementAsScalar<int>(sql, sqlParams);
		}
	}
}
