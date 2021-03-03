using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class AgentHelper : HelperBase
	{
		public AgentHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxyMock) : base(manager, database, proxyMock)
		{
		}

		public Agent CreateIntegrationPointAgent()
		{
			int artifactId = Artifact.NextId();

			var agent = new Agent
			{
				ArtifactId = artifactId,
				AgentTypeId = Const.Agent._INTEGRATION_POINTS_AGENT_TYPE_ID,
				AgentGuid = Const.Agent._RELATIVITY_INTEGRATION_POINTS_AGENT_GUID,
				FullNamespace = typeof(Agent).FullName,
				Name = $"Integration Points Agent ({artifactId})"
			};

			Database.Agents.Add(agent);

			return agent;
		}
	}
}
