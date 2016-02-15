using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers
{
	public abstract class ConsoleEventHandlerBase : kCura.EventHandler.ConsoleEventHandler
	{
		private IWorkspaceDBContext _workspaceDbContext;
		public IWorkspaceDBContext GetWorkspaceDbContext()
		{
			return _workspaceDbContext ??
			       (_workspaceDbContext = new WorkspaceContext(base.Helper.GetDBContext(base.Helper.GetActiveCaseID())));
		}
	}
}
