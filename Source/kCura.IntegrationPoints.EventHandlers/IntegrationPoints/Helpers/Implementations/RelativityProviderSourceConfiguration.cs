using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Artifact = kCura.EventHandler.Artifact;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class RelativityProviderSourceConfiguration : RelativityProviderConfiguration
    {
        private const string _ERROR_FOLDER_NOT_FOUND = "Folder in destination workspace not found!";
        private const string _ERROR_PRODUCTION_SET_NOT_FOUND = "Production Set not found!";
        private const string _SOURCE_RELATIVITY_INSTANCE = "SourceRelativityInstance";
        private const string _RELATIVITY_THIS_INSTANCE = "This instance";

        private readonly IProductionManager _productionManager;
        private readonly IManagerFactory _managerFactory;
        private readonly IInstanceSettingsManager _instanceSettingsManager;

        public RelativityProviderSourceConfiguration(
            IEHHelper helper,
            IProductionManager productionManager,
            IManagerFactory managerFactory,
            IInstanceSettingsManager instanceSettingsManager)
            : base(helper)
        {
            _productionManager = productionManager;
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
            SetViewName(settings);
        }

        private void SetInstanceFriendlyName(IDictionary<string, object> settings, IInstanceSettingsManager instanceSettingsManager)
        {
            settings[_SOURCE_RELATIVITY_INSTANCE] = $"{_RELATIVITY_THIS_INSTANCE}({instanceSettingsManager.RetriveCurrentInstanceFriendlyName()})";
        }

        private void SetSavedSearchName(IDictionary<string, object> settings)
        {
            int savedSearchArtifactId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId));
            if (savedSearchArtifactId > 0)
            {
                int sourceWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));
                SetSavedSearch(settings, sourceWorkspaceId, savedSearchArtifactId);
            }
        }

        private void SetSavedSearch(IDictionary<string, object> settings, int workspaceArtifactId, int savedSearchArtifactId)
        {
            KeywordSearchQueryResultSet queryResult = new GetSavedSearchQuery(Helper.GetServicesManager(), workspaceArtifactId, savedSearchArtifactId).ExecuteQuery();
            if (queryResult.Success && queryResult.Results != null && queryResult.Results.Count > 0)
            {
                settings[nameof(ExportUsingSavedSearchSettings.SavedSearch)] = queryResult.Results.Single().Artifact.Name;
            }
            else
            {
                settings[nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId)] = 0;
            }
        }

        private void SetViewName(IDictionary<string, object> settings)
        {
            int viewArtifactId = ParseValue<int>(settings, "SourceViewId");
            if (viewArtifactId > 0)
            {
                try
                {
                    int sourceWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId));
                    using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
                    {
                        QueryRequest request = new QueryRequest()
                        {
                            ObjectType = new ObjectTypeRef()
                            {
                                ArtifactTypeID = (int)ArtifactType.View
                            },
                            Condition = $"'ArtifactID' == {viewArtifactId}",
                            IncludeNameInQueryResult = true
                        };

                        QueryResult results = objectManager.QueryAsync(sourceWorkspaceId, request, 0, 1).GetAwaiter().GetResult();

                        if (results.TotalCount > 0)
                        {
                            settings["ViewName"] = results.Objects.First().Name;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Helper.GetLoggerFactory()
                        .GetLogger()
                        .ForContext<IntegrationPointViewPreLoad>()
                        .LogError(ex, "Source View not found");
                }
            }
        }

        private void SetSourceProductionName(IDictionary<string, object> settings)
        {
            const string sourceProductionId = "SourceProductionId";
            const string sourceProductionName = "SourceProductionName";
            try
            {
                int productionId = ParseValue<int>(settings, sourceProductionId);
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
            ProductionDTO production = _productionManager.RetrieveProduction(workspaceId, productionId);

            return production?.DisplayName;
        }

        private void SetTargetWorkspaceName(IDictionary<string, object> settings)
        {
            try
            {
                IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager();

                int targetWorkspaceId = ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId));

                WorkspaceDTO workspaceDTO = workspaceManager.RetrieveWorkspace(targetWorkspaceId);
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
                WorkspaceDTO workspaceDTO = workspaceManager.RetrieveWorkspace(sourceWorkspaceId);
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
            int productionArtifactId = ParseValue<int>(settings, nameof(ImportSettings.ProductionArtifactId));

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
                    List<Folder> folders = folderManager.GetFolderTreeAsync(targetWorkspaceId, new List<int>(), folderArtifactId).GetAwaiter().GetResult();
                    string folderName = FindFolderName(folders[0], folderArtifactId);
                    if (folderName == string.Empty)
                    {
                        folderName = _ERROR_FOLDER_NOT_FOUND;
                    }
                    settings[nameof(ExportUsingSavedSearchSettings.TargetFolder)] = folderName;
                }
                catch (Exception ex)
                {
                    Helper.GetLoggerFactory()
                        .GetLogger()
                        .ForContext<IntegrationPointViewPreLoad>()
                        .LogError(ex, _ERROR_FOLDER_NOT_FOUND);
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
