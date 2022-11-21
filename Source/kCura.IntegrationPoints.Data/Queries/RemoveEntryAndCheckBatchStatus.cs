using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class RemoveEntryAndCheckBatchStatus : IQuery<int>
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IWorkspaceDBContext _context;
        private readonly string _tableName;
        private readonly int _workspaceId;
        private readonly long _jobId;
        private readonly bool _batchIsFinished;

        public RemoveEntryAndCheckBatchStatus(
            IRepositoryFactory repositoryFactory,
            IWorkspaceDBContext context,
            string tableName,
            int workspaceId,
            long jobId,
            bool batchIsFinished)
        {
            _repositoryFactory = repositoryFactory;
            _context = context;
            _tableName = tableName;
            _workspaceId = workspaceId;
            _jobId = jobId;
            _batchIsFinished = batchIsFinished;
        }

        public int Execute()
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(_workspaceId, string.Empty, string.Empty);

            string sql = string.Format(
                Resources.Resource.RemoveEntryAndCheckBatchStatus,
                scratchTableRepository.GetResourceDBPrepend(),
                scratchTableRepository.GetSchemalessResourceDataBasePrepend(),
                _tableName);
            IList<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@jobID", _jobId),
                new SqlParameter("@batchIsFinished", _batchIsFinished ? 1 : 0)
            };
            return (int)_context.ExecuteSqlStatementAsScalar(sql, sqlParams.ToArray());
        }
    }
}
