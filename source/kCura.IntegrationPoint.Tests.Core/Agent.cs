using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter.Xml;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Services;
using Relativity.Services.Agent;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Agent
	{
		private const string _INTEGRATION_POINT_AGENT_TYPE_NAME = "Integration Points Agent";
		private const int _MAX_AGENT_TO_CREATE = 3;
		public static int CreateIntegrationPointAgent()
		{
			using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
			List<AgentTypeRef> agentTypes =	proxy.GetAgentTypesAsync().GetResultsWithoutContextSync();
				AgentTypeRef agentTypeRef = agentTypes.FirstOrDefault(agentType => agentType.Name == _INTEGRATION_POINT_AGENT_TYPE_NAME);
				if (agentTypeRef != null)
				{
					Query query = new Query();
					AgentQueryResultSet resultSet =	proxy.QueryAsync(query).GetResultsWithoutContextSync();
					global::Relativity.Services.Agent.Agent[] agents = resultSet.Results.Where(agent => agent.Success && agent.Artifact.AgentType.ArtifactID == agentTypeRef.ArtifactID).Select(result => result.Artifact).ToArray();
					if (agents.Length > _MAX_AGENT_TO_CREATE)
					{
						// returns 0, so we don't try to delete the agent at the end of the tests.
						return 0;
					}
				}

				List<ResourceServer> resourceServers = GetAgentServers();

				if (resourceServers.Count == 0)
				{
					throw new Exception($"Error: No Agent servers available for agent creation.");
				}

				ResourceServerRef resourceServerRef = new ResourceServerRef
				{
					ArtifactID = resourceServers[0].ArtifactID,
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
					int artifactId = proxy.CreateSingleAsync(agentDto).ConfigureAwait(false).GetAwaiter().GetResult();
					return artifactId;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed to create agent. Exception: {ex.Message}");
				}
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

		public static void UpdateIntegrationPointAgent(global::Relativity.Services.Agent.Agent agent)
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
			if(artifactId == 0) {  return; }
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

		private static AgentTypeRef GetAgentTypeByName(string agentTypeName)
		{
			List<AgentTypeRef> agentTypeRefs = GetAgentTypes();

			if (agentTypeRefs.IsNullOrEmpty())
			{
				throw new Exception("Failed to retrieve Agent Types. Please check the [AgentType] table on the primary server.");
			}

			foreach (AgentTypeRef agentTypeRef in agentTypeRefs)
			{
				if (agentTypeRef.Name == agentTypeName )
				{
					return agentTypeRef;
				}
			}

			throw new Exception($"Error: Did not find any matching AgentTypes with the name: {agentTypeName}.");
		}
	}
}