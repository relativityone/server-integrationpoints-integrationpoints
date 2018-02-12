using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.Web.Providers
{
	public class WorkspaceIdProvider : IWorkspaceIdProvider
	{
		private readonly IEnumerable<IWorkspaceService> _customPageServices;

		public WorkspaceIdProvider(IEnumerable<IWorkspaceService> services)
		{
			_customPageServices = services;
		}

		public int GetWorkspaceId()
		{
			return _customPageServices.First(x => x.GetWorkspaceID() != 0).GetWorkspaceID();
		}
	}
}
