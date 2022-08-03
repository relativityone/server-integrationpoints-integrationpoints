using kCura.IntegrationPoints.Data.Factories;
using System.Data;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class JobTrackerQueryManager : IJobTrackerQueryManager
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IWorkspaceDBContext _context;

        public JobTrackerQueryManager(IRepositoryFactory repositoryFactory, IWorkspaceDBContext context)
        {
            _repositoryFactory = repositoryFactory;
            _context = context;
        }

        public ICommand CreateJobTrackingEntry(string tableName, int workspaceId, long jobId)
        {
            return new CreateJobTrackingEntry(_repositoryFactory, _context, tableName, workspaceId, jobId);
        }

        public IQuery<DataTable> GetJobIdsFromTrackingEntry(string tableName, int workspaceId, long rootJobId)
        {
            return new GetJobIdsFromTrackingEntry(_repositoryFactory, _context, tableName, workspaceId, rootJobId);
        }

        public IQuery<int> RemoveEntryAndCheckBatchStatus(string tableName, int workspaceId, long jobId, bool isBatchFinished)
        {
            return new RemoveEntryAndCheckBatchStatus(_repositoryFactory, _context, tableName, workspaceId, jobId, isBatchFinished);
        }
    }
}
