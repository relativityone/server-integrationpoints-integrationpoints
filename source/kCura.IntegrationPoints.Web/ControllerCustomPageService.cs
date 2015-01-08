using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
			catch (NullReferenceException e)
			{
				return 0;
			}
			
		}
	}
}