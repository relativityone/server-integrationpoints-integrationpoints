using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class CreateJobTrackingEntry : ICommand
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IWorkspaceDBContext _context;
        private readonly string _tableName;
        private readonly int _workspaceId;
        private readonly long _jobId;

        public CreateJobTrackingEntry(
            IRepositoryFactory repositoryFactory,
            IWorkspaceDBContext context,
            string tableName,
            int workspaceId,
            long jobId)
        {
            _repositoryFactory = repositoryFactory;
            _context = context;
            _tableName = tableName;
            _workspaceId = workspaceId;
            _jobId = jobId;
        }

        public void Execute()
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(_workspaceId, string.Empty, string.Empty);

            string sql = string.Format(Resources.Resource.CreateJobTrackingEntry, scratchTableRepository.GetResourceDBPrepend(), _tableName);
            IList<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@jobID", _jobId)
            };
            _context.ExecuteNonQuerySQLStatement(sql, sqlParams);
        }
    }
}
