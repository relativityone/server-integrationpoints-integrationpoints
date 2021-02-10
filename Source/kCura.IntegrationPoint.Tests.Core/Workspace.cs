using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity;
using Relativity.Services;
using Relativity.Services.Exceptions;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ResourcePool;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Workspace
	{
		private static ITestHelper Helper => new TestHelper();

		public static Task<WorkspaceRef> GetWorkspaceAsync(int workspaceId) => GetWorkspaceAsync(x => x.ArtifactID == workspaceId);

		public static Task<WorkspaceRef> GetWorkspaceAsync(string workspaceName) => GetWorkspaceAsync(x => x.Name == workspaceName);

		public static Task<WorkspaceRef> CreateWorkspaceAsync(string name)
		{
			return CreateWorkspaceAsync(name, WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME);
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

		public static async Task<ResourcePool> GetWorkspaceResourcePoolAsync(int workspaceId)
		{
			using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
			using (IResourcePoolManager resourcePoolManager = Helper.CreateProxy<IResourcePoolManager>())
			{

				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
					Condition = $"'ArtifactID' == {workspaceId}",
					Fields = new List<FieldRef>()
					{
						new FieldRef {Name = "Resource Pool"}
					}
				};

				QueryResult workspaceResult = await objectManager.QueryAsync(-1, request, 0, 1).ConfigureAwait(false);

				string resourcePoolName = workspaceResult.Objects.Single()
					.FieldValues.Single().Value.ToString();

				Query resoucePoolByNameQuery = new Query
				{
					Condition = $"'Name' == '{resourcePoolName}'"
				};
				
				ResourcePoolQueryResultSet resourcePoolResult =
					await resourcePoolManager.QueryAsync(resoucePoolByNameQuery).ConfigureAwait(false);

				if (resourcePoolResult.TotalCount == 0)
				{
					return null;
				}

				return resourcePoolResult.Results.First().Artifact;
			}
		}

		private static async Task<WorkspaceRef> GetWorkspaceAsync(Func<WorkspaceRef, bool> workspaceFunc)
		{
			using (var workspaceManager = Helper.CreateProxy<IWorkspaceManager>())
			{
				IEnumerable<WorkspaceRef> activeWorkspaces = 
					await workspaceManager.RetrieveAllActive().ConfigureAwait(false);

				return activeWorkspaces.FirstOrDefault(workspaceFunc);
			}
		}
	}
}
