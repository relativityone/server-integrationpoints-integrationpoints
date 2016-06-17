using System;
using IAgentManager = Relativity.Services.Agent.IAgentManager;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Agent
	{

		//public static void CreateIntegrationPointAgent()
		//{
		//	using (IAgentManager proxy = Kepler.CreateProxy<IAgentManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
		//	{
		//		AgentDTO agentDto = new AgentDTO();
		//		int agentArtifactId = proxy.CreateSingleAsync(null);
		//		keywordSearch.SearchCriteria = searchCriteria;
		//		proxy.UpdateSingleAsync(workspaceArtifactId, keywordSearch).Wait((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
		//	}
		//}
	}
}
