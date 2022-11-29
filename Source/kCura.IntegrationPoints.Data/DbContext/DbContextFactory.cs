using kCura.IntegrationPoints.Common;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.DbContext
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly IHelper _helper;
        private readonly IAPILog _log;
        private readonly IRetryHandlerFactory _retryHandlerFactory;

        public DbContextFactory(IHelper helper, IAPILog log = null)
        {
            _helper = helper;

            _log = log ?? helper.GetLoggerFactory().GetLogger();

            _retryHandlerFactory = new RetryHandlerFactory(_log);
        }

        public IEddsDBContext CreatedEDDSDbContext()
        {
            return new EddsDBContext(_helper.GetDBContext(-1), _retryHandlerFactory);
        }

        public IWorkspaceDBContext CreateWorkspaceDbContext(int workspaceId)
        {
            return new WorkspaceDBContext(_helper.GetDBContext(workspaceId), _retryHandlerFactory);
        }
    }
}
