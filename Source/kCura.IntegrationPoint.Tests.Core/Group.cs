using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using Relativity.Services.Group;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Group
	{
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
				Console.WriteLine("An error occurred: {0}", ex.Message);
				throw;
			}

			// Check for success.
			if (!resultSet.Success)
			{
				Console.WriteLine("The Create operation failed.{0}{1}", Environment.NewLine, resultSet.Message);
				throw new Exception(resultSet.Message);
			}

			// Output the results.
			Console.WriteLine("The Create succeeded.");
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

		private const string _GET_WORKSPACE_GROUP = "api/Relativity.Services.Permission.IPermissionModule/Permission Manager/GetWorkspaceGroupSelectorAsync";
		private const string _ADD_REMOVE_WORKSPACE_GROUPS = "api/Relativity.Services.Permission.IPermissionModule/Permission Manager/AddRemoveWorkspaceGroupsAsync";

		public static void AddGroupToWorkspace(int workspaceId, int groupId)
		{
			string response = Rest.PostRequestAsJson(_GET_WORKSPACE_GROUP, $"{{workspaceArtifactID:{workspaceId}}}");
			GroupSelector groupSelector = JsonConvert.DeserializeObject<GroupSelector>(response);
			groupSelector.DisabledGroups = new List<GroupRef>();
			groupSelector.EnabledGroups = new List<GroupRef> { new GroupRef(groupId) };

			string parameter = $"{{workspaceArtifactID:{workspaceId},groupSelector:{JsonConvert.SerializeObject(groupSelector)}}}";
			Rest.PostRequestAsJson(_ADD_REMOVE_WORKSPACE_GROUPS, parameter);
		}

		public static void RemoveGroupFromWorkspace(int workspaceId, int groupId)
		{
			string response = Rest.PostRequestAsJson(_GET_WORKSPACE_GROUP, $"{{workspaceArtifactID:{workspaceId}}}");
			GroupSelector groupSelector = JsonConvert.DeserializeObject<GroupSelector>(response);
			groupSelector.DisabledGroups = new List<GroupRef> { new GroupRef(groupId) };

			string parameter = $"{{workspaceArtifactID:{workspaceId},groupSelector:{JsonConvert.SerializeObject(groupSelector)}}}";
			Rest.PostRequestAsJson(_ADD_REMOVE_WORKSPACE_GROUPS, parameter);
		}
	}
}