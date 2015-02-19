using System;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services.Tabs
{
	public class RSAPITabService : ITabService
	{
		private readonly IRSAPIClient _client;

		public RSAPITabService(IRSAPIClient client)
		{
			_client = client;
		}

		public int GetTabId(int objectTypeId)
		{
			var query = new Query<Tab>();
			query.Condition = new WholeNumberCondition(TabFieldNames.ObjectType, NumericConditionEnum.EqualTo, objectTypeId);
			var tab = _client.Repositories.Tab.Query(query);
			RDOHelper.CheckResult(tab);
			if (tab.Results.Count != 1)
			{
				throw new Exception(string.Format("Tab id for {0} did not return only one entry", objectTypeId) );
			}
			return tab.Results.First().Artifact.ArtifactID;
		}
	}
}
