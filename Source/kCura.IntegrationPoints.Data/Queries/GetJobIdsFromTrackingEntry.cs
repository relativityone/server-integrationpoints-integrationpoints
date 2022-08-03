using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class GetJobIdsFromTrackingEntry : IQuery<DataTable>
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IWorkspaceDBContext _context;
        private readonly string _tableName;
        private readonly int _workspaceId;
        private readonly long _rootJobId;

        public GetJobIdsFromTrackingEntry(IRepositoryFactory repositoryFactory, IWorkspaceDBContext context,
            string tableName, int workspaceId, long rootJobId)
        {
            _repositoryFactory = repositoryFactory;
            _context = context;
            _tableName = tableName;
            _workspaceId = workspaceId;
            _rootJobId = rootJobId;
        }

        public DataTable Execute()
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(_workspaceId, string.Empty, string.Empty);
            string scratchTableFullName = string.Format("{0}.[{1}]", scratchTableRepository.GetResourceDBPrepend(), _tableName);

            string sql = string.Format(Resources.Resource.GetJobIdsFromTrackingEntry, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME, scratchTableFullName);
            IList<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@jobID", _rootJobId)
            };

            return _context.ExecuteSqlStatementAsDataTable(sql, sqlParams);
        }
    }
}
