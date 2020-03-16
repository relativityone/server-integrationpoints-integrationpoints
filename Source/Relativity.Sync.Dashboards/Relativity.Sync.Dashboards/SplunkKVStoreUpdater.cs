﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Refit;
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
			await ClearKVCollectionAsync().ConfigureAwait(false);

			List<SyncIssueDTO> syncIssues = _syncIssues.ToList();
			List<SplunkKVCollectionItem> splunkKvCollectionItems = new List<SplunkKVCollectionItem>(syncIssues.Count);

			foreach (SyncIssueDTO syncIssue in syncIssues)
			{
				JiraTicketInfo jiraTicket = await GetJiraTicketAsync(syncIssue.Jira).ConfigureAwait(false);

				splunkKvCollectionItems.Add(new SplunkKVCollectionItem()
				{
					Jira = syncIssue.Jira,
					SearchMatch = syncIssue.SearchMatch,
					IssueType = jiraTicket.Fields.IssueType.Name,
					Status = jiraTicket.Fields.Status.Name,
					Summary = jiraTicket.Fields.Summary,
					Labels = jiraTicket.Fields.Labels
				});
			}

			foreach (SplunkKVCollectionItem item in splunkKvCollectionItems)
			{
				await AddSplunkKVItemAsync(item).ConfigureAwait(false);
			}
		}

		private Task ClearKVCollectionAsync()
		{
			try
			{
				_logger.LogInformation("Clearing Splunk KV collection: '{collectionName}'",
					_settings.SplunkKVCollectionName);
				return _splunkApi.ClearCollectionAsync(_settings.SplunkKVCollectionName);
			}
			catch (ApiException ex)
			{
				_logger.LogError(ex, "Encountered API error while clearing KV Store collection '{collectionName}': {content}",
					_settings.SplunkKVCollectionName, ex.Content);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception was thrown while clearing KV Store collection '{collectionName}'.",
					_settings.SplunkKVCollectionName);
				throw;
			}
		}

		private async Task<JiraTicketInfo> GetJiraTicketAsync(string jira)
		{
			try
			{
				_logger.LogInformation("Fetching Jira ticket information: {jira}", jira);
				JiraTicketInfo jiraTicket = await _jiraApi.GetIssueAsync(jira).ConfigureAwait(false);
				return jiraTicket;
			}
			catch (ApiException ex)
			{
				_logger.LogError(ex, "Encountered API error while fetching Jira {jira}: {content}",
					jira, ex.Content);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception was thrown while fetching Jira {jira}",
					jira);
				throw;
			}
		}

		private async Task AddSplunkKVItemAsync(SplunkKVCollectionItem item)
		{
			try
			{
				_logger.LogInformation("Creating new item in Splunk KV Store Collection for Jira: {jira}", item.Jira);
				await _splunkApi.UpdateLookupTableAsync(_settings.SplunkKVCollectionName, item)
					.ConfigureAwait(false);
			}
			catch (ApiException ex)
			{
				_logger.LogError(ex, "Encountered API error while updating Splunk KV Store: {content}", ex.Content);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception was thrown while adding item to KV Store collection");
				throw;
			}
		}
	}
}