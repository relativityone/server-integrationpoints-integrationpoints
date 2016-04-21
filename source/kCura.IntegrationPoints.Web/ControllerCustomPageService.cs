using System;
using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.Web
{
	public class ControllerCustomPageService : IWorkspaceService
	{
		public ISessionService Service { get; set; }

		public int GetWorkspaceID()
		{
			try
			{
				return Service.WorkspaceID;
			}
			//catch (NullReferenceException e)
			catch (Exception)
			{
				return 0;
			}
		}
	}
}