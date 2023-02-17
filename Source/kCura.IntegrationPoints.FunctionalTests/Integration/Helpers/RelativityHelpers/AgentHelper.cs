using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers
{
    public class AgentHelper : RelativityHelperBase
    {
        public AgentTest CreateIntegrationPointAgent()
        {
            int artifactId = ArtifactProvider.NextId();

            var agent = new AgentTest
            {
                ArtifactId = artifactId,
                AgentTypeId = Const.Agent.INTEGRATION_POINTS_AGENT_TYPE_ID,
                AgentGuid = Const.Agent.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID,
                FullNamespace = typeof(AgentTest).FullName,
                Name = $"Integration Points Agent ({artifactId})"
            };

            Relativity.Agents.Add(agent);

            return agent;
        }

        public AgentHelper(RelativityInstanceTest relativity) : base(relativity)
        {
        }
    }
}
