using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Agent;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Agent
	{
		private const string _INTEGRATION_POINT_AGENT_TYPE_NAME = "Integration Points Agent";

		public static int CreateIntegrationPointAgent()
		{
			using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				List<ResourceServer> resourceServers = GetAgentServers();
				ResourceServerRef resourceServerRef = new ResourceServerRef
				{
					ArtifactID = resourceServers[0].ArtifactID,
				};

				if (resourceServers.Count == 0)
				{
					throw new Exception($"Error: No Agent servers available for agent creation.");
				}

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
				throw new Exception("AgentTypeRefs is null or empty.");
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
