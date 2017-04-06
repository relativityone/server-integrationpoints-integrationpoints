using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Core;
using Relativity.Core.Service.Admin.ResourceServer;
using Relativity.Services;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class ResourceServerHelper
	{
		public static async Task<ResourceServer> GetAgentServer(ICoreContext coreContext)
		{
			using (var resourceServerManager = Kepler.CreateProxy<IResourceServerManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true))
			{
				var resourceServerTypes = new ResourceServerTypes(coreContext);
				var condition = new WholeNumberCondition("Type", NumericConditionEnum.EqualTo, new List<int>() { resourceServerTypes.Agent });
				var agentServerQuery = new Query()
				{
					Condition = condition.ToQueryString()
				};

				ResourceServerQueryResultSet agentServersQueryResults = await resourceServerManager.QueryAsync(agentServerQuery);
				if (!agentServersQueryResults.Results.Any())
				{
					throw new Exception("No Agent Servers Found");
				}

				return agentServersQueryResults.Results.First().Artifact;
			}
		}
	}
}