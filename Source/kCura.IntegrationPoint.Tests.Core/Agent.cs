using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.Client;
using Relativity.Services;
using Relativity.Services.Agent;
using Relativity.Services.ResourceServer;
using Query = Relativity.Services.Query;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Agent
	{
		private const string _INTEGRATION_POINT_AGENT_TYPE_NAME = "Integration Points Agent";
		private const int _MAX_NUMBER_OF_AGENTS_TO_CREATE = 4;

		private static ITestHelper Helper => new TestHelper();

		public static Result CreateIntegrationPointAgent()
		{
			global::Relativity.Services.Agent.Agent[] agents = GetIntegrationPointsAgents();

			if (agents.Length >= _MAX_NUMBER_OF_AGENTS_TO_CREATE)
			{
				return new Result
				{
					ArtifactID = agents[0].ArtifactID,
					Success = false
				};
			}

			return CreateIntegrationPointAgentInternal();
		}

		public static Result CreateIntegrationPointAgentIfNotExists()
		{
			global::Relativity.Services.Agent.Agent[] agents = GetIntegrationPointsAgents();

			if (agents.Any())
			{
				return new Result
				{
					ArtifactID = agents[0].ArtifactID,
					Success = false
				};
			}

			return CreateIntegrationPointAgentInternal();
		}

		private static global::Relativity.Services.Agent.Agent[] GetIntegrationPointsAgents()
		{
			AgentTypeRef agentTypeRef = GetAgentTypeByName(_INTEGRATION_POINT_AGENT_TYPE_NAME);
			Query query = new Query
			{
				Condition = $"'AgentTypeArtifactID' == {agentTypeRef.ArtifactID}"
			};
			AgentQueryResultSet resultSet = QueryAgents(query);
			return resultSet.Results
				.Where(agent => agent.Success)
				.Select(result => result.Artifact)
				.ToArray();
		}

		private static Result CreateIntegrationPointAgentInternal()
		{
			List<ResourceServer> resourceServers = GetAgentServers();

			if (!resourceServers.Any())
			{
				throw new Exception("Error: No Agent servers available for agent creation.");
			}

			ResourceServerRef resourceServerRef = new ResourceServerRef
			{
				ArtifactID = resourceServers[0].ArtifactID
			};

			global::Relativity.Services.Agent.Agent agentDto = new global::Relativity.Services.Agent.Agent
			{
				AgentType = GetAgentTypeByName(_INTEGRATION_POINT_AGENT_TYPE_NAME),
				Enabled = true,
				Interval = 5,
				LoggingLevel = global::Relativity.Services.Agent.Agent.LoggingLevelEnum.Critical,
				Server = resourceServerRef
			};

			try
			{
				using (
					IAgentManager proxy = Helper.CreateAdminProxy<IAgentManager>())
				{
					int artifactId = proxy.CreateSingleAsync(agentDto).ConfigureAwait(false).GetAwaiter().GetResult();

					return new Result
					{
						ArtifactID = artifactId,
						Success = true
					};
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error: Failed to create agent. Exception: {ex.Message}");
			}
		}

		public static void UpdateAgent(global::Relativity.Services.Agent.Agent agent)
		{
			using (IAgentManager proxy = Helper.CreateAdminProxy<IAgentManager>())
			{
				try
				{
					proxy.UpdateSingleAsync(agent).ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed to update agent. Exception: {ex.Message}");
				}
			}
		}

		public static List<ResourceServer> GetAgentServers()
		{
			using (IAgentManager proxy = Helper.CreateAdminProxy<IAgentManager>())
			{
				try
				{
					List<ResourceServer> resourceServers = proxy.GetAgentServersAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					return resourceServers;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed querying for agent servers. Exception: {ex.Message}");
				}
			}
		}

		public static void DeleteAgent(int artifactId)
		{
			if (artifactId == 0)
			{
				return;
			}
			using (IAgentManager proxy = Helper.CreateAdminProxy<IAgentManager>())
			{
				try
				{
					proxy.DeleteSingleAsync(artifactId).ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed deleting agent. Exception: {ex.Message}");
				}
			}
		}

		public static List<AgentTypeRef> GetAgentTypes()
		{
			using (IAgentManager proxy = Helper.CreateAdminProxy<IAgentManager>())
			{
				try
				{
					List<AgentTypeRef> agentTypeRefs = proxy.GetAgentTypesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					return agentTypeRefs;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed retrieving agent types. Exception: {ex.Message}");
				}
			}
		}

		public static AgentQueryResultSet QueryAgents(Query query)
		{
			using (IAgentManager proxy = Helper.CreateAdminProxy<IAgentManager>())
			{
				AgentQueryResultSet agentQueryResultSet = proxy.QueryAsync(query).ConfigureAwait(false).GetAwaiter().GetResult();

				if (agentQueryResultSet.Success == false)
				{
					throw new Exception($"Error: Failed querying for agents. Exception: {agentQueryResultSet.Message}");
				}

				return agentQueryResultSet;
			}
		}

		public static void DisableAgents(Query query)
		{
			AgentQueryResultSet agentQueryResultSet = QueryAgents(query);
			List<Result<global::Relativity.Services.Agent.Agent>> results = agentQueryResultSet.Results;
			foreach (Result<global::Relativity.Services.Agent.Agent> result in results)
			{
				result.Artifact.Enabled = false;
				UpdateAgent(result.Artifact);
			}
		}

		public static void DisableAllAgents()
		{
			Query query = GetAllIntegrationPointAgentsQuery();
			DisableAgents(query);
		}

		public static void EnableAgents(Query query)
		{
			AgentQueryResultSet agentQueryResultSet = QueryAgents(query);
			List<Result<global::Relativity.Services.Agent.Agent>> results = agentQueryResultSet.Results;
			foreach (Result<global::Relativity.Services.Agent.Agent> result in results)
			{
				result.Artifact.Enabled = true;
				UpdateAgent(result.Artifact);
			}
		}

		public static void EnableAllAgents()
		{
			Query query = GetAllIntegrationPointAgentsQuery();
			EnableAgents(query);
		}

		public static AgentTypeRef GetAgentTypeByName(string agentTypeName)
		{
			List<AgentTypeRef> agentTypeRefs = GetAgentTypes();

			if (agentTypeRefs.IsNullOrEmpty())
			{
				throw new Exception("Failed to retrieve Agent Types. Please check the [AgentType] table on the primary server.");
			}

			foreach (AgentTypeRef agentTypeRef in agentTypeRefs)
			{
				if (agentTypeRef.Name == agentTypeName)
				{
					return agentTypeRef;
				}
			}

			throw new Exception($"Error: Did not find any matching AgentTypes with the name: {agentTypeName}.");
		}

		private static Query GetAllIntegrationPointAgentsQuery()
		{
			AgentTypeRef agentTypeRef = GetAgentTypeByName(_INTEGRATION_POINT_AGENT_TYPE_NAME);

			if (agentTypeRef == null)
			{
				throw new Exception($"Agent with type name {_INTEGRATION_POINT_AGENT_TYPE_NAME} cannot be found");
			}

			Query query = new Query
			{
				Condition = $"'AgentTypeArtifactID' == {agentTypeRef.ArtifactID}"
			};
			return query;
		}
	}
}