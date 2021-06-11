using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class JobResourceTracker : IJobResourceTracker
    {
	    private static readonly object _syncRoot = new object();

		private readonly IRepositoryFactory _repositoryFactory;
        private readonly IWorkspaceDBContext _context;

        public JobResourceTracker(IRepositoryFactory repositoryFactory, IWorkspaceDBContext context)
        {
            _repositoryFactory = repositoryFactory;
            _context = context;
        }

        public void CreateTrackingEntry(string tableName, long jobId, int workspaceId)
        {
	        lock (_syncRoot)
	        {
		        IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceId, string.Empty, string.Empty);

		        string sql = string.Format(Resources.Resource.CreateJobTrackingEntry, scratchTableRepository.GetResourceDBPrepend(), tableName);
		        IList<SqlParameter> sqlParams = GetSqlParameters(jobId);
		        _context.ExecuteNonQuerySQLStatement(sql, sqlParams);
	        }
        }

        public int RemoveEntryAndCheckStatus(string tableName, long jobId, int workspaceId, bool batchIsFinished)
        {
	        lock (_syncRoot)
	        {
		        IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceId, string.Empty, string.Empty);

		        string sql = string.Format(Resources.Resource.RemoveEntryAndCheckBatchStatus, scratchTableRepository.GetResourceDBPrepend(), scratchTableRepository.GetSchemalessResourceDataBasePrepend(), tableName);
		        IList<SqlParameter> sqlParams = GetSqlParametersForRemoveAndCheck(jobId, batchIsFinished);
		        return _context.ExecuteSqlStatementAsScalar<int>(sql, sqlParams);
	        }
        }

        private IList<SqlParameter> GetSqlParameters(long jobId)
        {
	        return new List<SqlParameter>
	        {
		        new SqlParameter("@jobID", jobId)
	        };
        }

        private IList<SqlParameter> GetSqlParametersForRemoveAndCheck(long jobId, bool batchIsFinished)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@jobID", jobId),
                new SqlParameter("@batchIsFinished", batchIsFinished ? 1 : 0) 
            };
        }
    }
}
