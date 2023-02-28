using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Agent;
using Relativity.Services.ResourceServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using Query = Relativity.Services.Query;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class Agent
    {
        private const string _INTEGRATION_POINT_AGENT_TYPE_NAME = "Integration Points Agent";
        private const int _MAX_NUMBER_OF_AGENTS_TO_CREATE = 6;
        private static ITestHelper Helper => new TestHelper();

        public static async Task<bool> CreateMaxIntegrationPointAgentsAsync()
        {
            global::Relativity.Services.Agent.Agent[] agents = await GetIntegrationPointsAgentsAsync().ConfigureAwait(false);

            int agentsToCreate = _MAX_NUMBER_OF_AGENTS_TO_CREATE - agents.Length;
            for (int i = 0; i < agentsToCreate; ++i)
            {
                bool success = await CreateIntegrationPointAgentInternalAsync().ConfigureAwait(false);
                if (!success)
                {
                    return false;
                }
            }

            return true;
        }

        private static async Task<global::Relativity.Services.Agent.Agent[]> GetIntegrationPointsAgentsAsync()
        {
            AgentTypeRef agentTypeRef = await GetAgentTypeByNameAsync(_INTEGRATION_POINT_AGENT_TYPE_NAME).ConfigureAwait(false);
            var query = new Query
            {
                Condition = $"'AgentTypeArtifactID' == {agentTypeRef.ArtifactID}"
            };
            AgentQueryResultSet resultSet = await QueryAgentsAsync(query).ConfigureAwait(false);

            bool anyFailures = !resultSet.Success || resultSet.Results.Any(x => !x.Success);
            if (anyFailures)
            {
                throw new TestException($"Error: Cannot retrieve Integration Points agents, message: {resultSet.Message}");
            }

            return resultSet.Results
                .Where(agent => agent.Success)
                .Select(result => result.Artifact)
                .ToArray();
        }

        private static async Task<bool> CreateIntegrationPointAgentInternalAsync()
        {
            List<ResourceServer> resourceServers = await GetAgentServersAsync().ConfigureAwait(false);

            if (!resourceServers.Any())
            {
                throw new TestException("Error: No Agent servers available for agent creation.");
            }

            var resourceServerRef = new ResourceServerRef
            {
                ArtifactID = resourceServers[0].ArtifactID
            };

            var agentDto = new global::Relativity.Services.Agent.Agent
            {
                AgentType = await GetAgentTypeByNameAsync(_INTEGRATION_POINT_AGENT_TYPE_NAME).ConfigureAwait(false),
                Enabled = true,
                Interval = 5,
                LoggingLevel = global::Relativity.Services.Agent.Agent.LoggingLevelEnum.Critical,
                Server = resourceServerRef
            };

            try
            {
                using (IAgentManager proxy = Helper.CreateProxy<IAgentManager>())
                {
                    await proxy.CreateSingleAsync(agentDto).ConfigureAwait(false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new TestException($"Error: Failed to create agent. Exception: {ex.Message}", ex);
            }
        }

        private static async Task<AgentQueryResultSet> QueryAgentsAsync(Query query)
        {
            using (IAgentManager proxy = Helper.CreateProxy<IAgentManager>())
            {
                AgentQueryResultSet agentQueryResultSet = await proxy.QueryAsync(query).ConfigureAwait(false);

                if (!agentQueryResultSet.Success)
                {
                    throw new TestException($"Error: Failed querying for agents. Exception: {agentQueryResultSet.Message}");
                }

                return agentQueryResultSet;
            }
        }

        private static async Task<List<ResourceServer>> GetAgentServersAsync()
        {
            using (IAgentManager proxy = Helper.CreateProxy<IAgentManager>())
            {
                try
                {
                    List<ResourceServer> resourceServers = await proxy.GetAgentServersAsync().ConfigureAwait(false);
                    return resourceServers;
                }
                catch (Exception ex)
                {
                    throw new TestException($"Error: Failed querying for agent servers. Exception: {ex.Message}", ex);
                }
            }
        }

        private static async Task<AgentTypeRef> GetAgentTypeByNameAsync(string agentTypeName)
        {
            List<AgentTypeRef> agentTypeRefs = await GetAgentTypesAsync().ConfigureAwait(false);

            if (agentTypeRefs.IsNullOrEmpty())
            {
                throw new TestException("Failed to retrieve Agent Types. Please check the [AgentType] table on the primary server.");
            }

            foreach (AgentTypeRef agentTypeRef in agentTypeRefs)
            {
                if (agentTypeRef.Name == agentTypeName)
                {
                    return agentTypeRef;
                }
            }

            throw new TestException($"Error: Did not find any matching AgentTypes with the name: {agentTypeName}.");
        }

        private static async Task<List<AgentTypeRef>> GetAgentTypesAsync()
        {
            using (IAgentManager proxy = Helper.CreateProxy<IAgentManager>())
            {
                try
                {
                    List<AgentTypeRef> agentTypeRefs = await proxy.GetAgentTypesAsync().ConfigureAwait(false);
                    return agentTypeRefs;
                }
                catch (Exception ex)
                {
                    throw new TestException($"Error: Failed retrieving agent types. Exception: {ex.Message}", ex);
                }
            }
        }
    }
}
