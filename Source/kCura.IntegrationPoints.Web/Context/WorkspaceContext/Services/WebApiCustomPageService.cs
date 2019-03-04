using System.Web;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext.Services
{
	internal class WebApiCustomPageService : IWorkspaceService
	{
		private const string _WORKSPACE_ID_KEY = "workspaceID";

		private readonly HttpRequestBase _httpRequest;
		
		public WebApiCustomPageService(HttpRequestBase httpRequest)
		{
			_httpRequest = httpRequest;
		}

		public int GetWorkspaceID()
		{
			var context = _httpRequest.RequestContext.RouteData.Values[_WORKSPACE_ID_KEY] as string;
			int workspaceId = 0;
			int.TryParse(context, out workspaceId);
			return workspaceId;
		}
	}
}