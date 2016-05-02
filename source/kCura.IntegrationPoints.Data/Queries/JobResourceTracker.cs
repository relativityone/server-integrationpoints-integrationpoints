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
			string sql = Resources.Resource.CreateJobTrackingEntry;
			IList<SqlParameter> sqlParams = GetSqlParameters(tableName, jobId);
			_context.ExecuteNonQuerySQLStatement(sql, sqlParams);
		}

		public int RemoveEntryAndCheckStatus(string tableName, long jobId)
		{
			string sql = Resources.Resource.RemoveEntryAndCheckBatchStatus;
			IList<SqlParameter> sqlParams = GetSqlParameters(tableName, jobId);
			return _context.ExecuteSqlStatementAsScalar<int>(sql, sqlParams);
		}

		private IList<SqlParameter> GetSqlParameters(string tableName, long jobId)
		{
			IList<SqlParameter> sqlParameters = new List<SqlParameter>
			{
				new SqlParameter("@tableName", tableName),
				new SqlParameter("@jobID", jobId)
			};
			return sqlParameters;
		} 
	}
}
