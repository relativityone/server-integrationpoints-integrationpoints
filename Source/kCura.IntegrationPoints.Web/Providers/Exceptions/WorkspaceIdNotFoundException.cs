using System;

namespace kCura.IntegrationPoints.Web.Providers.Exceptions
{
	public class WorkspaceIdNotFoundException : InvalidOperationException
	{
		public WorkspaceIdNotFoundException() : base("WorkspaceId not found")
		{
		}
	}
}