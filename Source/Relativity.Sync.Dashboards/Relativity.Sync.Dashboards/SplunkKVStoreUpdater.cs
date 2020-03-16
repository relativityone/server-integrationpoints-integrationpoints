using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Relativity.Sync.Dashboards.Api;
using Relativity.Sync.Dashboards.Configuration;

namespace Relativity.Sync.Dashboards
{
	public class SplunkKVStoreUpdater
	{
		private readonly AppSettings _settings;
		private readonly IJiraApi _jiraApi;
		private readonly ISplunkApi _splunkApi;
		private readonly IEnumerable<SyncIssueDTO> _syncIssues;
		private readonly ILogger _logger;

		public SplunkKVStoreUpdater(AppSettings settings, IJiraApi jiraApi, ISplunkApi splunkApi, IEnumerable<SyncIssueDTO> syncIssues, ILogger logger)
		{
			_settings = settings;
			_jiraApi = jiraApi;
			_splunkApi = splunkApi;
			_syncIssues = syncIssues;
			_logger = logger;
		}

		public async Task UpdateSplunkKVStoreAsync()
		{
			foreach (SyncIssueDTO syncIssue in _syncIssues)
			{
				var result = await _jiraApi.GetIssueAsync(syncIssue.Jira).ConfigureAwait(false);
			}

			return;

			//await _splunkApi.ClearCollectionAsync(_settings.SplunkKVCollectionName).ConfigureAwait(false);

			//List<JiraTicketInfo> issues = new List<JiraTicketInfo>()
			//{
			//	new JiraTicketInfo()
			//	{
			//		Jira = "REL-123",
			//		SearchMatch = "\"*could not*\""
			//	}
			//};

			//foreach (JiraTicketInfo issue in issues)
			//{
			//	try
			//	{
			//		await _splunkApi.UpdateLookupTableAsync(_settings.SplunkKVCollectionName, JsonConvert.SerializeObject(issue)).ConfigureAwait(false);
			//	}
			//	catch (ApiException ex)
			//	{
			//		_logger.LogError(ex, "Error while updating Splunk KV Store: {content}", ex.Content);
			//		throw;
			//	}
			//}
		}
	}
}