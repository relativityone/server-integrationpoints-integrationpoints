using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class JobStatistics
	{
		public int Completed { get; set; }
		public int Errored { get; set; }
		public int Imported { get { return Completed - Errored; } }

		public static JobStatistics Populate(DataRow row)
		{
			var s = new JobStatistics();
			s.Completed = row.Field<int>("TotalRecords");
			s.Errored = row.Field<int>("ErrorRecords");
			return s;
		}
	}

	public class JobStatisticsQuery
	{
		private readonly IWorkspaceDBContext _context;

		public JobStatisticsQuery(IWorkspaceDBContext context)
		{
			_context = context;
		}

		public JobStatistics UpdateAndRetrieveStats(string tableName, long jobId, JobStatistics stats)
		{
			var sql = Resources.Resource.CreateJobTrackingEntry + Resources.Resource.UpdateJobStatistics;
			var p1 = new SqlParameter("@tableName", tableName);
			var p2 = new SqlParameter("@total", stats.Completed);
			var p3 = new SqlParameter("@errored", stats.Errored);
			var p4 = new SqlParameter("@jobID", jobId);
			var dt = _context.ExecuteSqlStatementAsDataTable(sql, new List<SqlParameter> { p1, p2, p3, p4 });
			return JobStatistics.Populate(dt.Rows[0]);
		}
	}
}
