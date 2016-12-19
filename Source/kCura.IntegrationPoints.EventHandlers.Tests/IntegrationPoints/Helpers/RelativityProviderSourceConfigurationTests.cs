using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
	public class RelativityProviderSourceConfigurationTests
	{
		[SetUp]
		public void SetUp()
		{
			_helper = Substitute.For<IEHHelper>();
			_workspaceRepository = Substitute.For<IWorkspaceRepository>();
			_instace = new RelativityProviderSourceConfiguration(_helper, _workspaceRepository);
		}

		private RelativityProviderSourceConfiguration _instace;
		private IEHHelper _helper;
		private IWorkspaceRepository _workspaceRepository;

		private const int _FOLDER_ARTIFACT_ID = 0;
		private const int _TARGET_WORKSPACE_ID = 1;
		private const int _SOURCE_WORKSPACE_ID = 2;
		private const int _SAVED_SEARCH_ARTIFACT_ID = 3;
		
		[TestCase("NewSourceWorkspaceName", "NewTargetWorkspaceName", "NewSavedSearchName", "NewFolderName", _FOLDER_ARTIFACT_ID)]
		[TestCase("NewSourceWorkspaceName", "NewTargetWorkspaceName", "NewSavedSearchName", RelativityProviderSourceConfiguration.ERROR_FOLDER_NOT_FOUND, -1)]
		public void ItShouldUpdateNames(string sourceWorkspaceName, string targetWorkspaceName, string savedSearchName, string folderName, int folderArtifactId)
		{
			// arrange
			var settings = GetSettings();
			MockFolderManager(folderName, folderArtifactId);
			MockWorkspaceRepository(_SOURCE_WORKSPACE_ID, sourceWorkspaceName);
			MockWorkspaceRepository(_TARGET_WORKSPACE_ID, targetWorkspaceName);
			MockSavedSearchQuery(savedSearchName);

			// act
			_instace.UpdateNames(settings);

			//assert
			Assert.AreEqual(sourceWorkspaceName, settings[nameof(ExportUsingSavedSearchSettings.SourceWorkspace)]);
			Assert.AreEqual(targetWorkspaceName, settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspace)]);
			Assert.AreEqual(savedSearchName, settings[nameof(ExportUsingSavedSearchSettings.SavedSearch)]);
			Assert.AreEqual(folderName, settings[nameof(ExportUsingSavedSearchSettings.TargetFolder)]);
		}

		private void MockSavedSearchQuery(string savedSearchName)
		{
			var field = new Field
			{
				Name = "Text Identifier",
				Value = savedSearchName
			};
			var artifact = new Artifact {Fields = new List<Field>() { field } };
			var queryResult = new QueryResult()
			{
				Success = true
			};
			queryResult.QueryArtifacts.Add(artifact);

			var rsapiClient = Substitute.For<IRSAPIClient>();
			rsapiClient.APIOptions = new APIOptions();
			rsapiClient.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(queryResult);
			_helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser).Returns(rsapiClient);
		}

		private void MockWorkspaceRepository(int workspaceId, string workspaceName)
		{
			var workspaceDto = new WorkspaceDTO() {Name = workspaceName};
			_workspaceRepository.Retrieve(workspaceId).Returns(workspaceDto);
		}

		private void MockFolderManager(string folderName, int folderArtifactId)
		{
			var folder = new Folder
			{
				ArtifactID = folderArtifactId,
				Name = folderName,
				Children = new List<Folder>()
			};
			var responseTask = Task.FromResult(new List<Folder>() {folder});

			var folderManager = Substitute.For<IFolderManager>();
			folderManager.GetFolderTreeAsync(_TARGET_WORKSPACE_ID, Arg.Any<List<int>>(), _FOLDER_ARTIFACT_ID).Returns(responseTask);
			_helper.GetServicesManager().CreateProxy<IFolderManager>(Arg.Any<ExecutionIdentity>()).Returns(folderManager);
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

			return settings;
		}
	}
}
