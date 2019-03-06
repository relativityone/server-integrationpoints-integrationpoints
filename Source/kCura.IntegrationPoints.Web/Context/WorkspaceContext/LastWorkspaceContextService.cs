using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
	internal class LastWorkspaceContextService : IWorkspaceContext
	{
		public int GetWorkspaceID()
		{
			throw new WorkspaceIdNotFoundException();
		}
	}
}