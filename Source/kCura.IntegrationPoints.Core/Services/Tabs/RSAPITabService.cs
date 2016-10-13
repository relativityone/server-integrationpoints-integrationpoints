using System;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Tabs
{
	public class RSAPITabService : ITabService
	{
		private readonly IRSAPIClient _client;
		private readonly IAPILog _logger;

		public RSAPITabService(IRSAPIClient client, IHelper helper)
		{
			_client = client;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RSAPITabService>();
		}

		public int GetTabId(int objectTypeId)
		{
			var query = new Query<Tab>();
			query.Condition = new WholeNumberCondition(TabFieldNames.ObjectType, NumericConditionEnum.EqualTo, objectTypeId);
			var tab = _client.Repositories.Tab.Query(query);
			tab.CheckResult();
			if (tab.Results.Count != 1)
			{
				LogRetrievingTabIdError(objectTypeId);
				throw new Exception(string.Format("Tab id for {0} did not return only one entry", objectTypeId));
			}
			return tab.Results.First().Artifact.ArtifactID;
		}

		#region Logging

		private void LogRetrievingTabIdError(int objectTypeId)
		{
			_logger.LogError("Tab id for {TabId} did not return only one entry.", objectTypeId);
		}

		#endregion
	}
}