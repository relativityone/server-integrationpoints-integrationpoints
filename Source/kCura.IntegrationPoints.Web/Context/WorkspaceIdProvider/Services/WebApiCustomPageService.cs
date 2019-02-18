using System.Web;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider.Services
{
	internal class WebApiCustomPageService : IWorkspaceService
	{
		private const string _WORKSPACE_ID_KEY = "workspaceID";

		public int GetWorkspaceID()
		{
			var context = HttpContext.Current.Request.RequestContext.RouteData.Values[_WORKSPACE_ID_KEY] as string;
			int workspaceId = 0;
			int.TryParse(context, out workspaceId);
			return workspaceId;
		}
	}
}