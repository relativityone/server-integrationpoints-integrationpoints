using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace kCura.IntegrationPoints.Web
{
	public class ControllerCustomPageService : ICustomPageService
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