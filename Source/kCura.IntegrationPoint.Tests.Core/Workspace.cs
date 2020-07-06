#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Serilog;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Workspace
	{
		public static int CreateWorkspace(string workspaceName)
		{
			return CreateWorkspace(workspaceName, WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME);
		}

		public static Task<int> CreateWorkspaceAsync(string workspaceName, string templateName, ILogger log = null)
		{
			return Task.Run(() => CreateWorkspace(workspaceName, templateName, log));
		}

		public static int CreateWorkspace(string workspaceName, string templateName, ILogger log = null)
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
						throw new Exception(
							$"Failed creating workspace {workspaceName}. Result Message: {result.Message} [{Environment.CurrentDirectory}]"
						);
					}

					Status.WaitForProcessToComplete(proxy, result.ProcessID, log: log);
					ProcessInformation processInfo = proxy.GetProcessState(proxy.APIOptions, result.ProcessID);
					workspaceId = processInfo.OperationArtifactIDs[0].GetValueOrDefault();
				}
				catch (Exception ex)
				{
					throw new Exception(
						$"An error occurred while creating workspace {workspaceName}. Error Message: {ex.Message}, error type: {ex.GetType()}",
						ex
					);
				}
			}

			return workspaceId;
		}

		public static bool CheckIfWorkspaceExists(string workspaceName)
		{
			if (string.IsNullOrEmpty(workspaceName))
			{
				throw new ArgumentException("Workspace name is not provided.");
			}

			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				QueryResultSet<Relativity.Client.DTOs.Workspace> queryResult = QueryWorkspaceByName(
					proxy,
					workspaceName
				);

				return queryResult.Results != null && queryResult.Results.Any();
			}
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

		public static void DeleteWorkspace(int workspaceArtifactId)
		{
			if (workspaceArtifactId == 0)
			{
				return;
			}
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

		public static Relativity.Client.DTOs.Workspace FindWorkspaceByName(IRSAPIClient proxy, string workspaceName)
		{
			try
			{
				return QueryWorkspaceByName(proxy, workspaceName).Results.Single().Artifact;
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
					throw new TestException($"An error occurred while retrieving workspace [{workspaceArtifactId}]. Error Message: {ex.Message}");
				}
			}
		}

		private static QueryResultSet<Relativity.Client.DTOs.Workspace> QueryWorkspaceByName(IRSAPIClient proxy, string workspaceName)
		{
			var workspaceNameCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, workspaceName);
			var query = new Query<Relativity.Client.DTOs.Workspace>
			{
				Condition = workspaceNameCondition
			};
			query.Fields.Add(new FieldValue(WorkspaceFieldNames.Name));
			return QueryWorkspace(proxy, query, 0);
		}

		private static QueryResultSet<Relativity.Client.DTOs.Workspace> QueryWorkspace(IRSAPIClient proxy, Query<Relativity.Client.DTOs.Workspace> query, int results)
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
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
