using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class JobResoureTracker
	{
		private readonly IWorkspaceDBContext _context;
		public JobResoureTracker(IWorkspaceDBContext context)
		{
			_context = context;
		}

		public void CreateTrackingEntry(string tableName, long jobId)
		{
			var sql = Resources.Resource.CreateJobTrackingEntry;
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@tableName", tableName));
			sqlParams.Add(new SqlParameter("@jobID", jobId));
			_context.ExecuteNonQuerySQLStatement(sql, sqlParams);
		}

		public int RemoveEntryAndCheckStatus(string tableName, long jobId)
		{
			var sql = Resources.Resource.RemoveEntryAndCheckBatchStatus;
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@tableName", tableName));
			sqlParams.Add(new SqlParameter("@jobID", jobId));
			return _context.ExecuteSqlStatementAsScalar<int>(sql, sqlParams);
		}
	}
}
