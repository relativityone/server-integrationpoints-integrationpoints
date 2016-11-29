﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class RelativityProviderSourceConfiguration : RelativityProviderConfiguration
	{
		private readonly IWorkspaceRepository _workspaceRepository;

		public RelativityProviderSourceConfiguration(IEHHelper helper, IWorkspaceRepository workspaceRepository) : base(helper)
		{
			_workspaceRepository = workspaceRepository;
		}

		public override void UpdateNames(IDictionary<string, object> settings)
		{
			SetSourceWorkspaceName(settings);
			SetTargetWorkspaceName(settings);
			SetSavedSearchName(settings);
		}

		private void SetSavedSearchName(IDictionary<string, object> settings)
		{
			int sourceWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));
			using (IRSAPIClient client = GetRsapiClient(sourceWorkspaceId))
			{
				var savedSearchArtifactId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId));
				if (savedSearchArtifactId > 0)
				{
					GetSavedSearchId(settings, client, savedSearchArtifactId);
				}
			}
		}

		private void GetSavedSearchId(IDictionary<string, object> settings, IRSAPIClient client, int savedSearchArtifactId)
		{
			var queryResult = new GetSavedSearchQuery(client, savedSearchArtifactId).ExecuteQuery();
			if (queryResult.Success)
			{
				settings[nameof(ExportUsingSavedSearchSettings.SavedSearch)] =
					queryResult.QueryArtifacts[0].getFieldByName("Text Identifier").ToString();
			}
			else
			{
				settings[nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId)] = 0;
			}
		}

		private void SetTargetWorkspaceName(IDictionary<string, object> settings)
		{
			try
			{
				int targetWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId));
				var workspaceDTO = _workspaceRepository.Retrieve(targetWorkspaceId);
				settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspace)] = workspaceDTO.Name;
			}
			catch (Exception ex)
			{
				Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointViewPreLoad>().LogError(ex, "Target workspace not found");
				settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId)] = 0;
			}
		}

		private void SetSourceWorkspaceName(IDictionary<string, object> settings)
		{
			int sourceWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));
			var workspaceDTO = _workspaceRepository.Retrieve(sourceWorkspaceId);
			settings[nameof(ExportUsingSavedSearchSettings.SourceWorkspace)] = workspaceDTO.Name;
		}
	}
}