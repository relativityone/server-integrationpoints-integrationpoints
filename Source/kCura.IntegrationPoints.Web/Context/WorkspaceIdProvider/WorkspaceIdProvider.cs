using System.Collections.Generic;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider.Services;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider.Services.Exceptions;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider
{
	internal class WorkspaceIdProvider : IWorkspaceIdProvider
	{
		private const int _WORKSPACE_NOT_FOUND_ID = 0;
		private readonly IEnumerable<IWorkspaceService> _workspaceServices;

		public WorkspaceIdProvider(IEnumerable<IWorkspaceService> workspaceServices)
		{
			_workspaceServices = workspaceServices;
		}

		public int GetWorkspaceId()
		{
			foreach (var workspaceService in _workspaceServices)
			{
				int workspaceId = workspaceService.GetWorkspaceID();

				if (workspaceId != _WORKSPACE_NOT_FOUND_ID)
				{
					return workspaceId;
				}
			}

			throw new WorkspaceIdNotFoundException();
		}
	}
}
