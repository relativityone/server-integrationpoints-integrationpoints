using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Folder;
using Artifact = kCura.EventHandler.Artifact;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class RelativityProviderSourceConfiguration : RelativityProviderConfiguration
	{
		private const string ERROR_FOLDER_NOT_FOUND = "Folder in destination workspace not found!";
		private const string SOURCE_RELATIVITY_INSTANCE = "SourceRelativityInstance";
		private const string RELATIVITY_THIS_INSTANCE = "This instance";
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IHelperFactory _helperFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IFederatedInstanceModelFactory _federatedInstanceModelFactory;
		private readonly IInstanceSettingsManager _instanceSettingsManager;


		public RelativityProviderSourceConfiguration(IEHHelper helper, IHelperFactory helperFactory, IManagerFactory managerFactory, IContextContainerFactory contextContainerFactory,
			IFederatedInstanceModelFactory federatedInstanceModelFactory, IInstanceSettingsManager instanceSettingsManager)
			: base(helper)
		{
			_helperFactory = helperFactory;
			_managerFactory = managerFactory;
			_contextContainerFactory = contextContainerFactory;
			_federatedInstanceModelFactory = federatedInstanceModelFactory;
			_instanceSettingsManager = instanceSettingsManager;
		}

		public override void UpdateNames(IDictionary<string, object> settings, Artifact artifact)
		{
			var federatedInstanceModel = _federatedInstanceModelFactory.Create(settings, artifact);
			SetFolderName(settings, federatedInstanceModel);
			SetInstanceFriendlyName(settings, _instanceSettingsManager);
			SetSourceWorkspaceName(settings);
			SetTargetWorkspaceName(settings, federatedInstanceModel);
			SetSavedSearchName(settings);
		}

		private void SetInstanceFriendlyName(IDictionary<string, object> settings, IInstanceSettingsManager federatedInstanceManager)
		{
			settings[SOURCE_RELATIVITY_INSTANCE] = $"{RELATIVITY_THIS_INSTANCE}({federatedInstanceManager.RetriveCurrentInstanceFriendlyName()})";
		}

		private void SetSavedSearchName(IDictionary<string, object> settings)
		{
			int sourceWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));
			using (IRSAPIClient client = GetRsapiClient(sourceWorkspaceId))
			{
				var savedSearchArtifactId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId));
				if (savedSearchArtifactId > 0)
					GetSavedSearchId(settings, client, savedSearchArtifactId);
			}
		}

		private void GetSavedSearchId(IDictionary<string, object> settings, IRSAPIClient client, int savedSearchArtifactId)
		{
			var queryResult = new GetSavedSearchQuery(client, savedSearchArtifactId).ExecuteQuery();
			if (queryResult.Success)
				settings[nameof(ExportUsingSavedSearchSettings.SavedSearch)] =
					queryResult.QueryArtifacts[0].getFieldByName("Text Identifier").ToString();
			else
				settings[nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId)] = 0;
		}

		private void SetTargetWorkspaceName(IDictionary<string, object> settings, FederatedInstanceModel federatedInstanceModel)
		{
			try
			{
				IHelper targetHelper = _helperFactory.CreateTargetHelper(Helper, federatedInstanceModel.FederatedInstanceArtifactId, federatedInstanceModel.Credentials);
				IWorkspaceManager workspaceManager =
					_managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(Helper,
						targetHelper.GetServicesManager()));

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
				IWorkspaceManager workspaceManager =
					_managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(Helper,
						Helper.GetServicesManager()));

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

		private void SetFolderName(IDictionary<string, object> settings, FederatedInstanceModel federatedInstanceModel)
		{
			IHelper targetHelper = _helperFactory.CreateTargetHelper(Helper, federatedInstanceModel.FederatedInstanceArtifactId, federatedInstanceModel.Credentials);

			using (IFolderManager folderManager =
				targetHelper.GetServicesManager().CreateProxy<IFolderManager>(ExecutionIdentity.CurrentUser))
			{
				int folderArtifactId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.FolderArtifactId));
				int targetWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId));
				try
				{
					var folders = folderManager.GetFolderTreeAsync(targetWorkspaceId, new List<int>(), folderArtifactId).Result;
					var folderName = FindFolderName(folders[0], folderArtifactId);
					if (folderName == string.Empty)
						folderName = ERROR_FOLDER_NOT_FOUND;
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
				return folder.Name;
			foreach (var folderChild in folder.Children)
			{
				var name = FindFolderName(folderChild, folderArtifactId);
				if (!string.IsNullOrEmpty(name))
					return $"{folder.Name}/{name}";
			}
			return string.Empty;
		}
	}
}