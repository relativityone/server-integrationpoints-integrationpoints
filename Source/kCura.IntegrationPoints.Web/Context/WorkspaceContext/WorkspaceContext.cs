using System.Collections.Generic;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Services;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
	internal class WorkspaceContext : IWorkspaceContext
	{
		private const int _WORKSPACE_NOT_FOUND_ID = 0;
		private readonly IEnumerable<IWorkspaceService> _workspaceServices;

		public WorkspaceContext(IEnumerable<IWorkspaceService> workspaceServices)
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
