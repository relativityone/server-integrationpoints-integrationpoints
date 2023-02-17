using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
    internal class NotFoundWorkspaceContextService : IWorkspaceContext
    {
        private readonly IAPILog _logger;

        public NotFoundWorkspaceContextService(IAPILog logger)
        {
            _logger = logger.ForContext<NotFoundWorkspaceContextService>();
        }

        public int GetWorkspaceID()
        {
            _logger.LogWarning("WorkspaceID not found in workspace context");
            throw new WorkspaceIdNotFoundException();
        }
    }
}
