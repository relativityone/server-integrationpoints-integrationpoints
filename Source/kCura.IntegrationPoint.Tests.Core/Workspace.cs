using System;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Workspace
	{
		public static int CreateWorkspace(string workspaceName, string templateName)
		{
			if (string.IsNullOrEmpty(workspaceName))
			{
				return 0; // TODO throw
			}

			//Create workspace DTO
			Relativity.Client.DTOs.Workspace workspaceDto = new Relativity.Client.DTOs.Workspace { Name = workspaceName };
			int workspaceId;
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					// Query for template workspace id
					Relativity.Client.DTOs.Workspace workspace = FindWorkspaceByName(proxy, templateName);
					int templateWorkspaceId = workspace.ArtifactID;

					ProcessOperationResult result = proxy.Repositories.Workspace.CreateAsync(templateWorkspaceId, workspaceDto);

					if (!result.Success)
					{
						throw new Exception($"Failed creating workspace {workspaceName}. Result Message: {result.Message} [{Environment.CurrentDirectory}]");
					}

					Status.WaitForProcessToComplete(proxy, result.ProcessID);
					ProcessInformation processInfo = proxy.GetProcessState(proxy.APIOptions, result.ProcessID);
					workspaceId = processInfo.OperationArtifactIDs[0].GetValueOrDefault();
				}
				catch (Exception ex)
				{
					throw new Exception($"An error occurred while creating workspace {workspaceName}. Error Message: {ex.Message}, error type: {ex.GetType()}", ex);
				}
			}
			return workspaceId;
		}

		public static void EnableDataGrid(int workspaceId)
		{
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					WorkspaceRepository workspaceRepository = proxy.Repositories.Workspace;

					Relativity.Client.DTOs.Workspace workspaceDTO = workspaceRepository.ReadSingle(workspaceId);

					workspaceDTO.EnableDataGrid = true;

					workspaceRepository.UpdateSingle(workspaceDTO);
				}
				catch (Exception ex)
				{
					throw new Exception($"An error occurred while updating workspace {workspaceId}. Error Message: {ex.Message}, error type: {ex.GetType()}", ex);
				}
			}
		}

		public static async Task<int> CreateWorkspaceAsync(string workspaceName, string templateName)
		{
			return await Task.Run(() => CreateWorkspace(workspaceName, templateName)).ConfigureAwait(false);
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
					throw new TestException($"An error occurred while deleting workspace [{workspaceArtifactId}]. Error Message: {ex.Message}");
				}
			}
		}

		public static bool IsWorkspacePresent(string workspaceName)
		{
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					var workspaceNameCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, workspaceName);
					var query = new Query<Relativity.Client.DTOs.Workspace>
					{
						Condition = workspaceNameCondition
					};
					var result = QueryWorkspace(proxy, query, 0);
					return result.TotalCount > 0;
				}
				catch (Exception ex)
				{
					throw new TestException($"An error occurred while retrieving workspace [{workspaceName}]. Error Message: {ex.Message}");
				}
			}
		}

		public static Relativity.Client.DTOs.Workspace FindWorkspaceByName(IRSAPIClient proxy, string workspaceName)
		{
			try
			{
				var workspaceNameCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, workspaceName);
				var query = new Query<Relativity.Client.DTOs.Workspace>
				{
					Condition = workspaceNameCondition
				};
				query.Fields.Add(new FieldValue(WorkspaceFieldNames.Name));
				Relativity.Client.DTOs.Workspace workspace = QueryWorkspace(proxy, query, 0).Results[0].Artifact;
				return workspace;
			}
			catch (Exception ex)
			{
				throw new TestException($"Finding workspace '{workspaceName}' failed.", ex);
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
						throw new TestException(result.Message);
					}
					if (result.Results.Count == 0)
					{
						throw new TestException("Unable to find the workspace.");
					}

					return result.Results[0].Artifact;
				}
				catch (Exception ex)
				{
					throw new TestException($"An error occurred while deleting workspace [{workspaceArtifactId}]. Error Message: {ex.Message}");
				}
			}
		}

		public static QueryResultSet<Relativity.Client.DTOs.Workspace> QueryWorkspace(IRSAPIClient proxy, Query<Relativity.Client.DTOs.Workspace> query, int results)
		{
			try
			{
				QueryResultSet<Relativity.Client.DTOs.Workspace> resultSet = proxy.Repositories.Workspace.Query(query, results);
				if (!resultSet.Success)
				{
					throw new TestException($"Query failed for workspace using Query: {query}");
				}
				return resultSet;
			}
			catch (Exception ex)
			{
				throw new TestException($"An error occurred attempting to query workspaces using query: {query}. Error Message: {ex.Message}", ex);
			}
		}
	}
}