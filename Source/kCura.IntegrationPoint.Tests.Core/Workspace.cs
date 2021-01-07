using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Exceptions;
using Relativity.Services.Folder;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Workspace
	{
		private static ITestHelper Helper => new TestHelper();

		public static async Task<WorkspaceRef> GetWorkspaceAsync(int workspaceId)
		{
			using (var workspaceManager = Helper.CreateProxy<IWorkspaceManager>())
			{
				IEnumerable<WorkspaceRef> activeWorkspaces =
					await workspaceManager.RetrieveAllActive().ConfigureAwait(false);

				return activeWorkspaces.Single(x => x.ArtifactID == workspaceId);
			}
		}

		public static async Task<WorkspaceRef> GetWorkspaceAsync(string workspaceName)
		{
			using (var workspaceManager = Helper.CreateProxy<IWorkspaceManager>())
			{
				IEnumerable<WorkspaceRef> activeWorkspaces =
					await workspaceManager.RetrieveAllActive().ConfigureAwait(false);

				return activeWorkspaces.FirstOrDefault(x => x.Name == workspaceName);
			}
		}

		public static async Task<WorkspaceRef> CreateWorkspaceAsync(string name)
		{
			return await CreateWorkspaceAsync(name, WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME);
		}

		public static async Task<WorkspaceRef> CreateWorkspaceAsync(string name, string templateWorkspaceName)
		{
			using (var workspaceManager = Helper.CreateProxy<IWorkspaceManager>())
			{
				IEnumerable<WorkspaceRef> activeWorkspaces = await workspaceManager.RetrieveAllActive().ConfigureAwait(false);
				WorkspaceRef template = activeWorkspaces.FirstOrDefault(x => x.Name == templateWorkspaceName);

				if (template == null)
				{
					throw new NotFoundException($"Cannot find workspace name: '{templateWorkspaceName}'");
				}

				WorkspaceRef workspaceRef = await workspaceManager.CreateWorkspaceAsync(new WorkspaceSetttings()
				{
					Name = name,
					TemplateArtifactId = template.ArtifactID,
					EnableDataGrid = true
				}).ConfigureAwait(false);

				return workspaceRef;
			}
		}

		public static async Task DeleteWorkspaceAsync(int workspaceId)
		{
			using (IWorkspaceManager workspaceManager = Helper.CreateProxy<IWorkspaceManager>())
			{
				WorkspaceRef workspace = new WorkspaceRef
				{
					ArtifactID = workspaceId
				};

				await workspaceManager.DeleteAsync(workspace).ConfigureAwait(false);
			}
		}

		public static async Task<int> GetRootFolderArtifactIDAsync(int workspaceId)
		{
			using (IFolderManager folderManager = Helper.CreateProxy<IFolderManager>())
			{
				Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
				return rootFolder.ArtifactID;
			}
		}

		public static async Task<FileShareResourceServer> GetDefaultWorkspaceFileShareServerIDAsync(int workspaceId)
		{
			using (IWorkspaceManager workspaceManager = Helper.CreateProxy<IWorkspaceManager>())
			{
				WorkspaceRef workspace = new WorkspaceRef
				{
					ArtifactID = workspaceId
				};

				return await workspaceManager.GetDefaultWorkspaceFileShareResourceServerAsync(workspace).ConfigureAwait(false);
			}
		}
	}
}
