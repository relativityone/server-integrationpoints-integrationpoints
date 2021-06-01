using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class JobStatistics
    {
        public int Completed { get; set; }
        public int Errored { get; set; }
        public int ImportApiErrors { get; set; }
        public int Imported { get { return Completed - ImportApiErrors; } }

        public static JobStatistics Populate(DataRow row)
        {
            var s = new JobStatistics();
            s.Completed = row.Field<int>("TotalRecords");
            s.Errored = row.Field<int>("ErrorRecords");
            s.ImportApiErrors = row.Field<int>("ImportApiErrors");
            return s;
        }
    }

    public interface IJobStatisticsQuery
    {
        JobStatistics UpdateAndRetrieveStats(string tableName, long jobId, JobStatistics stats, int workspaceID);
    }

    public class JobStatisticsQuery : IJobStatisticsQuery
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IWorkspaceDBContext _context;

        public JobStatisticsQuery(IRepositoryFactory repositoryFactory, IWorkspaceDBContext context)
        {
            _repositoryFactory = repositoryFactory;
            _context = context;
        }

        public JobStatistics UpdateAndRetrieveStats(string tableName, long jobId, JobStatistics stats, int workspaceID)
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
            string sql = string.Format(Resources.Resource.CreateJobTrackingEntry, scratchTableRepository.GetResourceDBPrepend(), tableName);
            sql += string.Format(Resources.Resource.UpdateJobStatistics, scratchTableRepository.GetResourceDBPrepend(), tableName);
            SqlParameter p1 = new SqlParameter("@total", stats.Completed);
            SqlParameter p2 = new SqlParameter("@errored", stats.Errored);
            SqlParameter p3 = new SqlParameter("@importApiErrors", stats.ImportApiErrors);
            SqlParameter p4 = new SqlParameter("@jobID", jobId);

            var dt = _context.ExecuteSqlStatementAsDataTable(sql, new List<SqlParameter> { p1, p2, p3, p4});
            return JobStatistics.Populate(dt.Rows[0]);
        }
    }
}
