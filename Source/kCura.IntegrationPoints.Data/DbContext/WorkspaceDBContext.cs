using kCura.IntegrationPoints.Common;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.DbContext
{
    public class WorkspaceDBContext : RipDbContextBase, IWorkspaceDBContext
    {
        public WorkspaceDBContext(IDBContext context, IRetryHandlerFactory retryHandlerFactory)
            : base(context, retryHandlerFactory)
        {
        }
    }
}
