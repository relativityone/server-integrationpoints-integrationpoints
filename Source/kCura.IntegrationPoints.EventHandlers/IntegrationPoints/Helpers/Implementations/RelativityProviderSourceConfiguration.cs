using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class RelativityProviderSourceConfiguration : IRelativityProviderSourceConfiguration
	{
		private readonly IHelper _helper;

		public RelativityProviderSourceConfiguration(IHelper helper)
		{
			_helper = helper;
		}

		public void UpdateNames(IDictionary<string, object> settings)
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
				var queryResult = new GetSavedSearchQuery(client, savedSearchArtifactId).ExecuteQuery();
				if (queryResult.Success)
				{
					settings[nameof(ExportUsingSavedSearchSettings.SavedSearch)] = queryResult.QueryArtifacts[0].getFieldByName("Text Identifier").ToString();
				}
				else
				{
					settings[nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId)] = 0;
				}
			}
		}

		private void SetTargetWorkspaceName(IDictionary<string, object> settings)
		{
			using (IRSAPIClient client = GetRsapiClient(-1))
			{
				int targetWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId));
				try
				{
					var targetWorkspace = client.Repositories.Workspace.ReadSingle(targetWorkspaceId);
					settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspace)] = targetWorkspace.Name;
				}
				catch (APIException ex)
				{
					_helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointViewPreLoad>().LogError(ex, "Target workspace not found");
					settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId)] = 0;
				}
			}
		}

		private void SetSourceWorkspaceName(IDictionary<string, object> settings)
		{
			using (IRSAPIClient client = GetRsapiClient(-1))
			{
				int sourceWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));
				var sourceWorkspace = client.Repositories.Workspace.ReadSingle(sourceWorkspaceId);
				settings[nameof(ExportUsingSavedSearchSettings.SourceWorkspace)] = sourceWorkspace.Name;
			}
		}

		protected virtual IRSAPIClient GetRsapiClient(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			return rsapiClient;
		}

		private T ParseValue<T>(IDictionary<string, object> settings, string parameterName)
		{
			if (!settings.ContainsKey(parameterName))
			{
				return default(T);
			}
			return (T) Convert.ChangeType(settings[parameterName], typeof(T));
		}
	}
}