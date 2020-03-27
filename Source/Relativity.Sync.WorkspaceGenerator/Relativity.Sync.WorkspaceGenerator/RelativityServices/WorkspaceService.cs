using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
	public class WorkspaceService
	{
		private readonly IServiceFactory _serviceFactory;

		public WorkspaceService(IServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<IEnumerable<WorkspaceRef>> GetAllActiveAsync()
		{
			using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				return await workspaceManager.RetrieveAllActive().ConfigureAwait(false);
			}
		}

		public async Task<WorkspaceRef> CreateWorkspaceAsync(string name, string templateWorkspaceName)
		{
			using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				IEnumerable<WorkspaceRef> activeWorkspaces = await GetAllActiveAsync().ConfigureAwait(false);
				WorkspaceRef template = activeWorkspaces.FirstOrDefault(x => x.Name == templateWorkspaceName);

				if (template == null)
				{
					throw new Exception($"Cannot find workspace name: '{templateWorkspaceName}'");
				}

				WorkspaceRef workspaceRef = await workspaceManager.CreateWorkspaceAsync(new WorkspaceSetttings()
				{
					Name = name,
					TemplateArtifactId = template.ArtifactID
				}).ConfigureAwait(false);

				return workspaceRef;
			}
		}
	}
}