using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class JobResourceTracker : IJobResourceTracker
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IWorkspaceDBContext _context;

        public JobResourceTracker(IRepositoryFactory repositoryFactory, IWorkspaceDBContext context)
        {
            _repositoryFactory = repositoryFactory;
            _context = context;
        }

        public void CreateTrackingEntry(string tableName, long jobId, int workspaceID)
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
            string sql = string.Format(Resources.Resource.CreateJobTrackingEntry, scratchTableRepository.GetResourceDBPrepend(), tableName);
            IList<SqlParameter> sqlParams = GetSqlParameters(jobId);
            _context.ExecuteNonQuerySQLStatement(sql, sqlParams);
        }

        public int RemoveEntryAndCheckStatus(string tableName, long jobId, int workspaceID)
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
            string sql = string.Format(Resources.Resource.RemoveEntryAndCheckBatchStatus, scratchTableRepository.GetResourceDBPrepend(), scratchTableRepository.GetSchemalessResourceDataBasePrepend(), tableName);
            IList<SqlParameter> sqlParams = GetSqlParameters(jobId);
            return _context.ExecuteSqlStatementAsScalar<int>(sql, sqlParams);
        }

        private IList<SqlParameter> GetSqlParameters(long jobId)
        {
            IList<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@jobID", jobId)
            };
            return sqlParameters;
        }
    }
}
