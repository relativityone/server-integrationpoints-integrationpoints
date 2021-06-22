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

        public int GetProcessingBatchesCount(string tableName, long rootJobId, int workspaceId)
        {
	        lock (_syncRoot)
	        {
		        IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceId, string.Empty, string.Empty);
		        string scratchTableFullName = string.Format("{0}.[{1}]", scratchTableRepository.GetResourceDBPrepend(), tableName);
                
                string sql = string.Format(Resources.Resource.GetProcessingSyncWorkerBatches, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME, scratchTableFullName);
		        IList<SqlParameter> sqlParams = GetSqlParameters(rootJobId);
		        return _context.ExecuteSqlStatementAsDataTable(sql, sqlParams).Rows.Count;
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
