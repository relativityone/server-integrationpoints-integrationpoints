using System.Collections.Generic;
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

		public async Task<WorkspaceRef> CreateWorkspaceAsync(string name, int templateWorkspaceArtifactID)
		{
			using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				WorkspaceRef workspaceRef = await workspaceManager.CreateWorkspaceAsync(new WorkspaceSetttings()
				{
					Name = name,
					TemplateArtifactId = templateWorkspaceArtifactID
				}).ConfigureAwait(false);

				return workspaceRef;
			}
		}
	}
}