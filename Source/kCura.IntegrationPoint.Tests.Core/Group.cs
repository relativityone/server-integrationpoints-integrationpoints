using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Interfaces.Group.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Group
	{
		private static ITestHelper Helper => new TestHelper();

		public static int CreateGroup(string name)
		{
			using (IGroupManager groupManager = Helper.CreateProxy<IGroupManager>())
			using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
			{
				var clientRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef() {ArtifactTypeID = (int) ArtifactType.Client}
				};
				
				QueryResultSlim result = objectManager.QuerySlimAsync(-1, clientRequest, 0, int.MaxValue)
					.GetAwaiter().GetResult();
				
				if(result.ResultCount == 0)
				{
					throw new NotFoundException("There is no client in the instance");
				}

				int clientArtifactId = result.Objects.First().ArtifactID;

				GroupRequest request = new GroupRequest
				{
					Name = name,
					Client = new Securable<ObjectIdentifier>(new ObjectIdentifier { ArtifactID = clientArtifactId })
				};

				return groupManager.CreateAsync(request).GetAwaiter().GetResult()
					.ArtifactID;
			}
		}

		public static void DeleteGroup(int artifactId)
		{
			using (IGroupManager groupManager = Helper.CreateProxy<IGroupManager>())
			{
				groupManager.DeleteAsync(artifactId).GetAwaiter().GetResult();
			}
		}

		public static void AddGroupToWorkspace(int workspaceId, int groupId)
		{
			AddGroupToWorkspaceAsync(workspaceId, groupId).GetAwaiter().GetResult();
		}

		public static void RemoveGroupFromWorkspace(int workspaceId, int groupId)
		{
			RemoveGroupFromWorkspaceAsync(workspaceId, groupId).GetAwaiter().GetResult();
		}

		private static async Task AddGroupToWorkspaceAsync(int workspaceId, int groupId)
		{
			using (var proxy = Helper.CreateProxy<IPermissionManager>())
			{
				GroupSelector groupSelector = await proxy.GetWorkspaceGroupSelectorAsync(workspaceId).ConfigureAwait(false);
				groupSelector.DisabledGroups = new List<GroupRef>();
				groupSelector.EnabledGroups = new List<GroupRef> { new GroupRef(groupId) };

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).ConfigureAwait(false);
			}
		}

		private static async Task RemoveGroupFromWorkspaceAsync(int workspaceId, int groupId)
		{
			using (var proxy = Helper.CreateProxy<IPermissionManager>())
			{
				GroupSelector groupSelector = await proxy.GetWorkspaceGroupSelectorAsync(workspaceId).ConfigureAwait(false);
				groupSelector.DisabledGroups = new List<GroupRef> { new GroupRef(groupId) };

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).ConfigureAwait(false);
			}
		}
	}
}
