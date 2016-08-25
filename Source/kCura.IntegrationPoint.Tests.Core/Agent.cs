﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
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

		public static Result CreateIntegrationPointAgent()
		{
			Result agentCreatedResult;

			AgentTypeRef agentTypeRef = GetAgentTypeByName(_INTEGRATION_POINT_AGENT_TYPE_NAME);

			if (agentTypeRef == null)
			{
				throw new Exception($"Agent with type name {_INTEGRATION_POINT_AGENT_TYPE_NAME} cannot be found");
			}

			Query query = new Query
			{
				Condition = $"'AgentTypeArtifactID' == {agentTypeRef.ArtifactID}"
			};
			AgentQueryResultSet resultSet = QueryAgents(query);
			global::Relativity.Services.Agent.Agent[] agents = resultSet.Results.Where(agent => agent.Success).Select(result => result.Artifact).ToArray();

			if (agents.Length >= _MAX_NUMBER_OF_AGENTS_TO_CREATE)
			{
				agentCreatedResult = new Result
				{
					ArtifactID = agents[0].ArtifactID,
					Success = false
				};
				return agentCreatedResult;
			}

			List<ResourceServer> resourceServers = GetAgentServers();

			if (resourceServers.Count == 0)
			{
				throw new Exception("Error: No Agent servers available for agent creation.");
			}

			ResourceServerRef resourceServerRef = new ResourceServerRef
			{
				ArtifactID = resourceServers[0].ArtifactID,
			};

			global::Relativity.Services.Agent.Agent agentDto = new global::Relativity.Services.Agent.Agent
			{
				AgentType = agentTypeRef,
				Enabled = true,
				Interval = 5,
				LoggingLevel = global::Relativity.Services.Agent.Agent.LoggingLevelEnum.Critical,
				Server = resourceServerRef
			};

			try
			{
				using (
					IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName,
						SharedVariables.RelativityPassword, true, true))
				{
					int artifactId = proxy.CreateSingleAsync(agentDto).ConfigureAwait(false).GetAwaiter().GetResult();

					agentCreatedResult = new Result
					{
						ArtifactID = artifactId,
						Success = true
					};
					return agentCreatedResult;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error: Failed to create agent. Exception: {ex.Message}");
			}
		}

		public static global::Relativity.Services.Agent.Agent ReadIntegrationPointAgent(int agentArtifactId)
		{
			using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				try
				{
					global::Relativity.Services.Agent.Agent agent = proxy.ReadSingleAsync(agentArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();
					return agent;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed to read agent. Exception: {ex.Message}");
				}
			}
		}

		public static void UpdateAgent(global::Relativity.Services.Agent.Agent agent)
		{
			using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
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
			using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
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
			if (artifactId == 0) { return; }
			using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
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
			using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
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
			using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
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
			AgentTypeRef agentTypeRef = GetAgentTypeByName(_INTEGRATION_POINT_AGENT_TYPE_NAME);

			if (agentTypeRef == null)
			{
				throw new Exception($"Agent with type name {_INTEGRATION_POINT_AGENT_TYPE_NAME} cannot be found");
			}

			Query query = new Query
			{
				Condition = $"'AgentTypeArtifactID' == {agentTypeRef.ArtifactID}"
			};

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
	}
}