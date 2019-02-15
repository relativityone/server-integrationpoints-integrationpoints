using System;

namespace kCura.IntegrationPoints.Web.RelativityServices.Exceptions
{
	public class WorkspaceIdNotFoundException : InvalidOperationException
	{
		public WorkspaceIdNotFoundException() : base("WorkspaceId not found")
		{
		}
	}
}