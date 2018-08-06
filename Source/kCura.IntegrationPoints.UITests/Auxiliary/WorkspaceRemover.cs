using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using System;
using System.Net;

namespace kCura.IntegrationPoints.UITests.Auxiliary
{
	public class WorkspaceRemover
	{

		public static void Main()
		{
			new WorkspaceRemover().Init().DeleteTestWorkspaces();
		}

		public WorkspaceRemover Init()
		{
			new WindsorContainer();

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
			                                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			new DefaultConfigurationStore();

			new TestConfiguration()
				.MergeCustomConfigWithAppSettings()
				.SetupConfiguration()
				.LogConfiguration();

			return this;
		}

		public void DeleteTestWorkspaces()
		{
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				foreach (string beginningOfName in new[] {"RIP Test Workspace"})
				{
					DeleteTestWorkspace(proxy, beginningOfName);
				}
			}
		}

		private static void DeleteTestWorkspace(IRSAPIClient proxy, string beginningOfName)
		{
			try
			{
				var workspaceNameCondition =
					new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.StartsWith, beginningOfName);
				var query = new Query<Relativity.Client.DTOs.Workspace>
				{
					Condition = workspaceNameCondition
				};

				const int resultMaxLength = 10000;
				QueryResultSet<Relativity.Client.DTOs.Workspace> resultSet = proxy.Repositories.Workspace.Query(query, resultMaxLength);
				if (!resultSet.Success)
				{
					throw new TestException($"Query failed for workspace using Query: {query}, {resultSet.Message}");
				}

				Console.WriteLine($@"Found {resultSet.TotalCount}");

				foreach (Result<Relativity.Client.DTOs.Workspace> res in resultSet.Results)
				{
					Relativity.Client.DTOs.Workspace ws = res.Artifact;
					Console.WriteLine(@"ws: " + ws.Name + @" " + ws.ArtifactID + @" by " + ws.SystemCreatedBy);
					WriteResultSet<Relativity.Client.DTOs.Workspace> rs = proxy.Repositories.Workspace.Delete(ws.ArtifactID);
					if (!rs.Success)
					{
						Console.WriteLine($@"Error while deleting ws: {rs.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				throw new TestException($@"An error occurred. Error Message: {ex.Message}");
			}
		}
	}
}
