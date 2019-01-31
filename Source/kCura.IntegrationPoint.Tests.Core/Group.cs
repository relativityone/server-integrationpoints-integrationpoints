using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Group
	{
		private static ITestHelper Helper => new TestHelper();

		public static int CreateGroup(string name)
		{
			// STEP 1: Create a DTO and set its properties.
			kCura.Relativity.Client.DTOs.Group newGroup = new kCura.Relativity.Client.DTOs.Group
			{
				Name = name
			};

			// STEP 2: Create a WriteResultSet. It provide details after the create operation completes.
			WriteResultSet<kCura.Relativity.Client.DTOs.Group> resultSet;

			// STEP 3: Create the new Group.
			try
			{
				using (IRSAPIClient rsapiClient = Rsapi.CreateRsapiClient())
				{
					resultSet = rsapiClient.Repositories.Group.Create(newGroup);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($@"An error occurred while creating group {name}: {ex.Message}");
				throw;
			}

			// Check for success.
			if (!resultSet.Success)
			{
				Console.WriteLine($@"Creation of group {name} failed.{Environment.NewLine}{resultSet.Message}");
				throw new Exception(resultSet.Message);
			}

			// Output the results.
			Console.WriteLine($@"The group {name} created succeessfully.");
			kCura.Relativity.Client.DTOs.Group createdGroup = resultSet.Results[0].Artifact;

			Console.WriteLine("{0}The Artifact of the New Group is: {1}", Environment.NewLine, createdGroup.ArtifactID);

			return createdGroup.ArtifactID;
		}

		public static bool DeleteGroup(int artifactId)
		{
			// STEP 1: Create a DTO populated with criteria for a DTO you want to delete.
			kCura.Relativity.Client.DTOs.Group groupToDelete = new kCura.Relativity.Client.DTOs.Group(artifactId);

			// STEP 2: Create a WriteResultSet. It provides details after the delete operation completes.
			WriteResultSet<kCura.Relativity.Client.DTOs.Group> resultSet;

			// STEP 3: Perform the delete operation.
			try
			{
				using (IRSAPIClient rsapiClient = Rsapi.CreateRsapiClient())
				{
					resultSet = rsapiClient.Repositories.Group.Delete(groupToDelete);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: {0}", ex.Message);
				return false;
			}

			// Check for success.
			if (!resultSet.Success)
			{
				Console.WriteLine("The Delete operation failed.{0}{1}", Environment.NewLine, resultSet.Message);
				return false;
			}

			// Output the results.
			Console.WriteLine("Delete completed successfully.");
			kCura.Relativity.Client.DTOs.Group deletedGroup = resultSet.Results.ElementAt(0).Artifact;
			Console.WriteLine("The Artifact ID of the deleted Group is: {0}", deletedGroup.ArtifactID);

			return true;
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
			using (var proxy = Helper.CreateAdminProxy<IPermissionManager>())
			{
				GroupSelector groupSelector = await proxy.GetWorkspaceGroupSelectorAsync(workspaceId).ConfigureAwait(false);
				groupSelector.DisabledGroups = new List<GroupRef>();
				groupSelector.EnabledGroups = new List<GroupRef> { new GroupRef(groupId) };

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).ConfigureAwait(false);
			}
		}

		private static async Task RemoveGroupFromWorkspaceAsync(int workspaceId, int groupId)
		{
			using (var proxy = Helper.CreateAdminProxy<IPermissionManager>())
			{
				GroupSelector groupSelector = await proxy.GetWorkspaceGroupSelectorAsync(workspaceId).ConfigureAwait(false);
				groupSelector.DisabledGroups = new List<GroupRef> { new GroupRef(groupId) };

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).ConfigureAwait(false);
			}
		}
	}
}