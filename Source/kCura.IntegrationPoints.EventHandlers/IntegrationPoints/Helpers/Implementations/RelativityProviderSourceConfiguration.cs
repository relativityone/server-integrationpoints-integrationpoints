#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Folder;
using Artifact = kCura.EventHandler.Artifact;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class RelativityProviderSourceConfiguration : RelativityProviderConfiguration
	{
		private const string ERROR_FOLDER_NOT_FOUND = "Folder in destination workspace not found!";
		private const string _ERROR_PRODUCTION_SET_NOT_FOUND = "Production Set not found!";
		private const string SOURCE_RELATIVITY_INSTANCE = "SourceRelativityInstance";
		private const string RELATIVITY_THIS_INSTANCE = "This instance";
		private readonly Func<IProductionManager> _productionManagerFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IInstanceSettingsManager _instanceSettingsManager;

		public RelativityProviderSourceConfiguration(
			IEHHelper helper,
			Func<IProductionManager> productionManagerFactory,
			IManagerFactory managerFactory,
			IInstanceSettingsManager instanceSettingsManager)
			: base(helper)
		{
			_productionManagerFactory = productionManagerFactory;
			_managerFactory = managerFactory;
			_instanceSettingsManager = instanceSettingsManager;
		}

		public override void UpdateNames(IDictionary<string, object> settings, Artifact artifact)
		{
			SetLocationName(settings);
			SetTargetWorkspaceName(settings);
			SetInstanceFriendlyName(settings, _instanceSettingsManager);
			SetSourceWorkspaceName(settings);
			SetSavedSearchName(settings);
			SetSourceProductionName(settings);
		}

		private void SetInstanceFriendlyName(IDictionary<string, object> settings, IInstanceSettingsManager instanceSettingsManager)
		{
			settings[SOURCE_RELATIVITY_INSTANCE] = $"{RELATIVITY_THIS_INSTANCE}({instanceSettingsManager.RetriveCurrentInstanceFriendlyName()})";
		}

		private void SetSavedSearchName(IDictionary<string, object> settings)
		{
			var savedSearchArtifactId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId));
			if (savedSearchArtifactId > 0)
			{
				int sourceWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));
				using (IRSAPIClient client = GetRsapiClient(sourceWorkspaceId))
				{
					GetSavedSearchId(settings, client, savedSearchArtifactId);
				}
			}
		}

		private void GetSavedSearchId(IDictionary<string, object> settings, IRSAPIClient client, int savedSearchArtifactId)
		{
			QueryResult queryResult = new GetSavedSearchQuery(client, savedSearchArtifactId).ExecuteQuery();
			if (queryResult.Success && queryResult.QueryArtifacts != null && queryResult.QueryArtifacts.Count > 0)
			{
				if (queryResult.QueryArtifacts != null && queryResult.QueryArtifacts.Count > 0)
				{
					Field savedSearchField = queryResult.QueryArtifacts[0].getFieldByName("Text Identifier");
					if (savedSearchField != null)
					{
						settings[nameof(ExportUsingSavedSearchSettings.SavedSearch)] = savedSearchField.ToString();
					}

				}
			}
			else
			{
				settings[nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId)] = 0;
			}
		}

		private void SetSourceProductionName(IDictionary<string, object> settings)
		{
			const string sourceProductionId = "SourceProductionId";
			const string sourceProductionName = "SourceProductionName";
			try
			{
				var productionId = ParseValue<int>(settings, sourceProductionId);
				if (productionId > 0)
				{
					int sourceWorkspaceId = ParseValue<int>(settings,
						nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));

					settings[sourceProductionName] = GetProductionSetNameById(sourceWorkspaceId, productionId);
				}
			}
			catch (Exception ex)
			{
				Helper.GetLoggerFactory()
					.GetLogger()
					.ForContext<IntegrationPointViewPreLoad>()
					.LogError(ex, "Source Production Set not found");
				settings[sourceProductionId] = 0;
			}
		}

		private string GetProductionSetNameById(int workspaceId, int productionId)
		{
			IProductionManager productionManager = _productionManagerFactory();

			ProductionDTO production = productionManager.RetrieveProduction(workspaceId, productionId);

			return production?.DisplayName;
		}

		private void SetTargetWorkspaceName(IDictionary<string, object> settings)
		{
			try
			{
				IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager();

				int targetWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId));

				var workspaceDTO = workspaceManager.RetrieveWorkspace(targetWorkspaceId);
				settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspace)] = workspaceDTO.Name;
			}
			catch (Exception ex)
			{
				Helper.GetLoggerFactory()
					.GetLogger()
					.ForContext<IntegrationPointViewPreLoad>()
					.LogError(ex, "Target workspace not found");
				settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId)] = 0;
			}
		}

		private void SetSourceWorkspaceName(IDictionary<string, object> settings)
		{
			try
			{
				IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager();

				int sourceWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));
				var workspaceDTO = workspaceManager.RetrieveWorkspace(sourceWorkspaceId);
				settings[nameof(ExportUsingSavedSearchSettings.SourceWorkspace)] = workspaceDTO.Name;
			}
			catch (Exception ex)
			{
				Helper.GetLoggerFactory()
					.GetLogger()
					.ForContext<IntegrationPointViewPreLoad>()
					.LogError(ex, "Source workspace not found");
				settings[nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId)] = 0;
			}
		}

		private void SetLocationName(IDictionary<string, object> settings)
		{
			int folderArtifactId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.FolderArtifactId));
			var productionArtifactId = ParseValue<int>(settings, nameof(ImportSettings.ProductionArtifactId));

			if (folderArtifactId == 0 && productionArtifactId > 0)
			{
				SetDestinationProductionSetName(settings, productionArtifactId);
			}
			else if (folderArtifactId > 0 && productionArtifactId == 0)
			{
				SetFolderName(settings, folderArtifactId);
			}
		}

		private void SetDestinationProductionSetName(IDictionary<string, object> settings, int productionArtifactId)
		{
			const string targetProductionSet = "targetProductionSet";
			try
			{
				int targetWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId));


				string productionSetName = GetProductionSetNameById(targetWorkspaceId, productionArtifactId);

				if (productionSetName == string.Empty)
				{
					productionSetName = _ERROR_PRODUCTION_SET_NOT_FOUND;
				}
				settings[targetProductionSet] = productionSetName;
			}
			catch (Exception ex)
			{
				Helper.GetLoggerFactory()
					.GetLogger()
					.ForContext<IntegrationPointViewPreLoad>()
					.LogError(ex, "Destination Production Set not found");
				settings[nameof(ImportSettings.ProductionArtifactId)] = 0;
			}
		}

		private void SetFolderName(IDictionary<string, object> settings, int folderArtifactId)
		{
			using (IFolderManager folderManager = Helper.GetServicesManager().CreateProxy<IFolderManager>(ExecutionIdentity.CurrentUser))
			{
				int targetWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId));

				try
				{
					List<Folder> folders = folderManager.GetFolderTreeAsync(targetWorkspaceId, new List<int>(), folderArtifactId).Result;
					string folderName = FindFolderName(folders[0], folderArtifactId);
					if (folderName == string.Empty)
					{
						folderName = ERROR_FOLDER_NOT_FOUND;
					}
					settings[nameof(ExportUsingSavedSearchSettings.TargetFolder)] = folderName;
				}
				catch (Exception ex)
				{
					Helper.GetLoggerFactory()
						.GetLogger()
						.ForContext<IntegrationPointViewPreLoad>()
						.LogError(ex, ERROR_FOLDER_NOT_FOUND);
					settings[nameof(ExportUsingSavedSearchSettings.FolderArtifactId)] = 0;
				}
			}
		}

		private string FindFolderName(Folder folder, int folderArtifactId)
		{
			if (folder.ArtifactID == folderArtifactId)
			{
				return folder.Name;
			}
			foreach (var folderChild in folder.Children)
			{
				string name = FindFolderName(folderChild, folderArtifactId);
				if (!string.IsNullOrEmpty(name))
				{
					return $"{folder.Name}/{name}";
				}
			}
			return string.Empty;
		}
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
