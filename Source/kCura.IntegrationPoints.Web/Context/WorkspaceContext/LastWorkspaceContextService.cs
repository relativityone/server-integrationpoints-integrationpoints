using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
	internal class LastWorkspaceContextService : IWorkspaceContext
	{
		public int GetWorkspaceId()
		{
			throw new WorkspaceIdNotFoundException();
		}
	}
}