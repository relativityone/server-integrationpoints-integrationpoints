using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Folder;
using Relativity.Services.Search;
using Folder = Relativity.Services.Folder.Folder;
using Query = Relativity.Services.Query;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class RelativityProviderSourceConfigurationTests
    {
        private RelativityProviderSourceConfiguration _sut;
        private IEHHelper _helper;
        private IManagerFactory _managerFactory;
        private IWorkspaceManager _workspaceManager;
        private IInstanceSettingsManager _instanceSettingsManager;
        private IProductionManager _productionManagerMock;
        private const int _FOLDER_ARTIFACT_ID = 123456;
        private const int _TARGET_WORKSPACE_ID = 1;
        private const int _SOURCE_WORKSPACE_ID = 2;
        private const int _SAVED_SEARCH_ARTIFACT_ID = 3;
        private const int _PRODUCTION_ID = 4;
        private const string _ERROR_FOLDER_NOT_FOUND = "Folder in destination workspace not found!";
        private const string _SOURCE_RELATIVITY_INSTANCE = "SourceRelativityInstance";
        private const string _RELATIVITY_THIS_INSTANCE = "This instance";
        private const string _SOURCE_PRODUCTION_NAME = "SourceProductionName";
        private const string _SOURCE_PRODUCTION_ID = "SourceProductionId";

        [SetUp]
        public void SetUp()
        {
            _helper = Substitute.For<IEHHelper>();
            _managerFactory = Substitute.For<IManagerFactory>();
            _workspaceManager = Substitute.For<IWorkspaceManager>();
            _instanceSettingsManager = Substitute.For<IInstanceSettingsManager>();
            _productionManagerMock = Substitute.For<IProductionManager>();

            _sut = new RelativityProviderSourceConfiguration(_helper, _productionManagerMock, _managerFactory, _instanceSettingsManager);
        }

        [TestCase("NewSourceWorkspaceName", "NewTargetWorkspaceName", "NewSavedSearchName", "NewFolderName", _FOLDER_ARTIFACT_ID, "FriendlyName", "SourceProductionName")]
        [TestCase("NewSourceWorkspaceName", "NewTargetWorkspaceName", "NewSavedSearchName", _ERROR_FOLDER_NOT_FOUND, -1, "FriendlyName", "SourceProductionName")]
        public void ItShouldUpdateNames(string sourceWorkspaceName, string targetWorkspaceName, string savedSearchName, string folderName, int folderArtifactId,
            string instanceFriendlyName, string productionName)
        {
            // arrange
            var settings = GetSettings();
            MockWorkspaceRepository(_SOURCE_WORKSPACE_ID, sourceWorkspaceName);
            MockWorkspaceRepository(_TARGET_WORKSPACE_ID, targetWorkspaceName);
            MockFolderManager(folderName, folderArtifactId);
            MockSavedSearchQuery(savedSearchName);
            MockInstanceSettingsManager(instanceFriendlyName);
            MockProductionName(productionName);

            // act
            _sut.UpdateNames(settings, new EventHandler.Artifact(934580, 990562, 533988, "", false, null));

            // assert
            Assert.AreEqual(sourceWorkspaceName, settings[nameof(ExportUsingSavedSearchSettings.SourceWorkspace)]);
            Assert.AreEqual(targetWorkspaceName, settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspace)]);
            Assert.AreEqual(savedSearchName, settings[nameof(ExportUsingSavedSearchSettings.SavedSearch)]);
            Assert.AreEqual(folderName, settings[nameof(ExportUsingSavedSearchSettings.TargetFolder)]);
            Assert.AreEqual($"{_RELATIVITY_THIS_INSTANCE}({instanceFriendlyName})", settings[_SOURCE_RELATIVITY_INSTANCE]);
            Assert.AreEqual(productionName, settings[_SOURCE_PRODUCTION_NAME]);
        }

        private void MockProductionName(string productionName)
        {
            var productionDto = new ProductionDTO()
            {
                DisplayName = productionName
            };

            _productionManagerMock.RetrieveProduction(_SOURCE_WORKSPACE_ID, _PRODUCTION_ID).Returns(productionDto);
        }

        private void MockSavedSearchQuery(string savedSearchName)
        {
            var queryResult = new KeywordSearchQueryResultSet()
            {
                Success = true,
                Results = new List<Result<KeywordSearch>>()
                {
                    new Result<KeywordSearch>()
                    {
                        Artifact = new KeywordSearch()
                        {
                            Name = savedSearchName
                        }
                    }
                }
            };

            IKeywordSearchManager keywordSearchManager = Substitute.For<IKeywordSearchManager>();
            keywordSearchManager.QueryAsync(Arg.Any<int>(), Arg.Any<Query>()).Returns(queryResult);
            _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.CurrentUser).Returns(keywordSearchManager);
        }

        private void MockWorkspaceRepository(int workspaceId, string workspaceName)
        {
            var workspaceDto = new WorkspaceDTO() { Name = workspaceName };

            _workspaceManager.RetrieveWorkspace(workspaceId).Returns(workspaceDto);
            _managerFactory.CreateWorkspaceManager().Returns(_workspaceManager);
        }

        private void MockFolderManager(string folderName, int folderArtifactId)
        {
            var folder = new Folder
            {
                ArtifactID = folderArtifactId,
                Name = folderName,
                Children = new List<Folder>()
            };
            var responseTask = Task.FromResult(new List<Folder>() { folder });

            var folderManager = Substitute.For<IFolderManager>();
            folderManager.GetFolderTreeAsync(_TARGET_WORKSPACE_ID, Arg.Any<List<int>>(), _FOLDER_ARTIFACT_ID).Returns(responseTask);
            _helper.GetServicesManager().CreateProxy<IFolderManager>(Arg.Any<ExecutionIdentity>()).Returns(folderManager);
        }

        private void MockInstanceSettingsManager(string instanceFriendlyName)
        {
            _instanceSettingsManager.RetriveCurrentInstanceFriendlyName().Returns(instanceFriendlyName);
        }

        private IDictionary<string, object> GetSettings()
        {
            var settings = new Dictionary<string, object>();

            settings.Add(nameof(ExportUsingSavedSearchSettings.SourceWorkspace), string.Empty);
            settings.Add(nameof(ExportUsingSavedSearchSettings.TargetWorkspace), string.Empty);
            settings.Add(nameof(ExportUsingSavedSearchSettings.SavedSearch), string.Empty);
            settings.Add(nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId), _SAVED_SEARCH_ARTIFACT_ID);
            settings.Add(nameof(ExportUsingSavedSearchSettings.TargetFolder), string.Empty);
            settings.Add(nameof(ExportUsingSavedSearchSettings.FolderArtifactId), _FOLDER_ARTIFACT_ID);
            settings.Add(nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId), _TARGET_WORKSPACE_ID);
            settings.Add(nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId), _SOURCE_WORKSPACE_ID);
            settings.Add(_SOURCE_PRODUCTION_NAME, string.Empty);
            settings.Add(_SOURCE_PRODUCTION_ID, _PRODUCTION_ID);

            return settings;
        }
    }
}
