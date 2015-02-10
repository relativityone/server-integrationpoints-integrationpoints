using System.Web;
using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.Web
{
	public class WebAPICustomPageService : IWorkspaceService
	{
		public int GetWorkspaceID()
		{
			var context = HttpContext.Current.Request.RequestContext.RouteData.Values["workspaceID"] as string;
			var workspaceID = 0;
			int.TryParse(context, out workspaceID);
			return workspaceID;
		}
	}
}