using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.ResourcePool;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class ResourcePoolHelper
	{
		public static async Task<ResourcePool> GetResourcePool(string resourcePoolName)
		{
			using (var proxy = Kepler.CreateProxy<IResourcePoolManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true))
			{
				var condition = new TextCondition("Name", TextConditionEnum.EqualTo, resourcePoolName);
				var query = new Query() { Condition = condition.ToQueryString() };

				var resourcePoolQueryResultSet = await proxy.QueryAsync(query);
				var resourcePools = resourcePoolQueryResultSet;

				if (!resourcePools.Results.Any())
				{
					throw new Exception($"No Resource Pools with name: {resourcePoolName} found");
				}

				return resourcePools.Results.First().Artifact;
			}
		}

		public static async Task<List<ResourceServerRef>> GetServersConnectedToResourcePool(ResourcePool resourcePool)
		{
			using (var resourcePoolManager = Kepler.CreateProxy<IResourcePoolManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true))
			{
				var resourcePoolRef = new ResourcePoolRef() { ArtifactID = resourcePool.ArtifactID, Name = resourcePool.Name };
				return await resourcePoolManager.RetrieveResourceServersAsync(resourcePoolRef);
			}
		}

		public static async Task AddAgentServerToResourcePool(ResourceServer agentServer, string resourcePoolName)
		{
			ResourcePool resourcePool = await GetResourcePool(resourcePoolName);
			List<ResourceServerRef> serversConnectedToResourcePool = await GetServersConnectedToResourcePool(resourcePool);

			if (serversConnectedToResourcePool.All(x => x.ArtifactID != agentServer.ArtifactID))
			{
				using (var resourcePoolManager = Kepler.CreateProxy<IResourcePoolManager>(SharedVariables.RelativityUserName,SharedVariables.RelativityPassword, true))
				{
					ResourceServerRef agentServerToAdd = new ResourceServerRef()
					{
						ArtifactID = agentServer.ArtifactID,
						Name = agentServer.Name,
						ServerType = agentServer.ServerType
					};

					ResourcePoolRef resourcePoolRef = new ResourcePoolRef()
					{
						ArtifactID = resourcePool.ArtifactID,
						Name = resourcePool.Name
					};

					await resourcePoolManager.AddServerAsync(agentServerToAdd, resourcePoolRef);
				}
			}
		}
	}
}