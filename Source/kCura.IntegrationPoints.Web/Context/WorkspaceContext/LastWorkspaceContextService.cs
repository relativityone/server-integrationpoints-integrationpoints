using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
	internal class LastWorkspaceContextService : IWorkspaceContext
	{
		private readonly IAPILog _logger;

		public LastWorkspaceContextService(IAPILog logger)
		{
			_logger = logger.ForContext<LastWorkspaceContextService>();
		}

		public int GetWorkspaceID()
		{
			_logger.LogWarning("WorkspaceID not found in workspace context");
			throw new WorkspaceIdNotFoundException();
		}
	}
}