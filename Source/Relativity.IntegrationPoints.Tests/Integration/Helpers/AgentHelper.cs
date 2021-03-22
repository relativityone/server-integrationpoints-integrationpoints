using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class AgentHelper : HelperBase
	{
		public AgentHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxyMock) : base(manager, database, proxyMock)
		{
		}

		public AgentTest CreateIntegrationPointAgent()
		{
			int artifactId = Artifact.NextId();

			var agent = new AgentTest
			{
				ArtifactId = artifactId,
				AgentTypeId = Const.Agent.INTEGRATION_POINTS_AGENT_TYPE_ID,
				AgentGuid = Const.Agent.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID,
				FullNamespace = typeof(AgentTest).FullName,
				Name = $"Integration Points Agent ({artifactId})"
			};

			Database.Agents.Add(agent);

			return agent;
		}
	}
}
