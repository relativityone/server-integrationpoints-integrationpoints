using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class ResourceServerHelper
	{
		private const string _AGENT_SERVER_TYPE_NAME = "Agent";
		private const string _TYPE_FIELD_NAME = "Type";

		private static ITestHelper Helper => new TestHelper();

		public static async Task<ResourceServer> GetAgentServerAsync()
		{
			using (var resourceServerManager = Helper.CreateProxy<IResourceServerManager>())
			{
				List<ResourceServerTypeRef> serverTypes = await resourceServerManager
					.RetrieveAllServerTypesAsync()
					.ConfigureAwait(false);

				int agentServerTypeID = serverTypes
					.First(x => x.Name == _AGENT_SERVER_TYPE_NAME)
					.ArtifactID;

				Query agentServerTypeQuery = BuildAgentServerTypeQuery(agentServerTypeID);

				ResourceServerQueryResultSet agentServersQueryResults = await resourceServerManager
					.QueryAsync(agentServerTypeQuery)
					.ConfigureAwait(false);

				return agentServersQueryResults.Results.First().Artifact;
			}
		}

		private static Query BuildAgentServerTypeQuery(int agentServerTypeID)
		{
			var condition = new WholeNumberCondition(
				_TYPE_FIELD_NAME,
				NumericConditionEnum.EqualTo,
				new List<int> { agentServerTypeID }
			);
			return new Query
			{
				Condition = condition.ToQueryString()
			};
		}
	}
}