using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Workspace
	{
		public static void ImportApplicationToWorkspace(int workspaceId, string applicationFilePath, bool forceUnlock, List<int> appsToOverride = null)
		{
			//List of application ArtifactIDs to override, if already installed
			// TODO: Add this functionality - Gerron Thurman 5/11/2016
			List<int> applicationsToOverride = appsToOverride ?? new List<int>();

			AppInstallRequest appInstallRequest = new AppInstallRequest()
			{
				FullFilePath = applicationFilePath,
				ForceFlag = true
			};

			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					proxy.APIOptions.WorkspaceID = workspaceId;
					ProcessOperationResult result = proxy.InstallApplication(proxy.APIOptions, appInstallRequest);
					if (!result.Success)
					{
						throw new Exception(string.Format("Failed to install application file: {0} to workspace: {1}.", applicationFilePath, workspaceId));
					}

					Status.WaitForProcessToComplete(proxy, result.ProcessID, 300, 500);
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("An error occurred attempting to import the application file {0}. Error: {1}.", applicationFilePath, ex.Message));
				}
			}
		}

		public static int CreateWorkspace(string workspaceName, string templateName)
		{
			if (String.IsNullOrEmpty(workspaceName)) return 0;

			//Create workspace DTO
			Relativity.Client.DTOs.Workspace workspaceDto = new Relativity.Client.DTOs.Workspace { Name = workspaceName };
			int workspaceId = 0;
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					//Query for template workspace id
					TextCondition workspaceNameCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, templateName);
					Query<Relativity.Client.DTOs.Workspace> query = new Query<Relativity.Client.DTOs.Workspace>
					{
						Condition = workspaceNameCondition
					};
					query.Fields.Add(new FieldValue(WorkspaceFieldNames.Name));
					int templateWorkspaceId = QueryWorkspace(query, 0).Results[0].Artifact.ArtifactID;

					ProcessOperationResult result;

					result = proxy.Repositories.Workspace.CreateAsync(templateWorkspaceId, workspaceDto);

					if (!result.Success)
					{
						throw new Exception(string.Format("Failed creating workspace {0}. Result Message: {1}", workspaceName, result.Message));
					}

					Status.WaitForProcessToComplete(proxy, result.ProcessID);
					ProcessInformation processInfo = proxy.GetProcessState(proxy.APIOptions, result.ProcessID);
					workspaceId = processInfo.OperationArtifactIDs[0].GetValueOrDefault();
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("An error occurred while creating workspace {0}. Error Message: {1}", workspaceName, ex.Message));
				}
			}
			return workspaceId;
		}

		public static void DeleteWorkspace(int workspaceArtifactId)
		{
			if (workspaceArtifactId == 0) return;
			//Create workspace DTO
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					proxy.Repositories.Workspace.Delete(workspaceArtifactId);
				}
				catch (Exception ex)
				{
					throw new Exception($"An error occurred while deleting workspace [{workspaceArtifactId}]. Error Message: {ex.Message}");
				}
			}
		}

		public static Relativity.Client.DTOs.Workspace GetWorkspaceDto(int workspaceArtifactId)
		{
			//Create workspace DTO
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					ResultSet<Relativity.Client.DTOs.Workspace> result = proxy.Repositories.Workspace.Read(workspaceArtifactId);
					if (result.Success == false)
					{
						throw new Exception(result.Message);
					}
					else if (result.Results.Count == 0)
					{
						throw new Exception("Unable to find the workspace.");
					}

					return result.Results[0].Artifact;
				}
				catch (Exception ex)
				{
					throw new Exception($"An error occurred while deleting workspace [{workspaceArtifactId}]. Error Message: {ex.Message}");
				}
			}
		}

		public static QueryResultSet<Relativity.Client.DTOs.Workspace> QueryWorkspace(Query<Relativity.Client.DTOs.Workspace> query, int results)
		{
			QueryResultSet<Relativity.Client.DTOs.Workspace> resultSet = new QueryResultSet<Relativity.Client.DTOs.Workspace>();
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					resultSet = proxy.Repositories.Workspace.Query(query, results);
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("An error occurred attempting to query workspaces using query: {0}. Error Message: {1}", query, ex.Message));
				}

				if (!resultSet.Success)
				{
					throw new Exception(string.Format("Query failed for workspace using Query: {0}", query));
				}

				return resultSet;
			}
		}
	}
}