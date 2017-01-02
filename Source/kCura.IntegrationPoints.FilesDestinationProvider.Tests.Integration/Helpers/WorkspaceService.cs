using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class WorkspaceService
	{
		#region Constructors

		public WorkspaceService(ImportHelper importHelper)
		{
			_importHelper = importHelper;
		}

		#endregion //Constructors

		#region Fields

		private readonly ImportHelper _importHelper;

		private const string _TEMPLATE_WORKSPACE_NAME = "kCura Starter Template";
		private const string _SAVED_SEARCH_FOLDER = "Testing Folder";
		private const string _SAVED_SEARCH_NAME = "Testing Saved Search";

		#endregion //Fields

		#region Methods

		internal int CreateWorkspace(string name)
		{
			return Workspace.CreateWorkspace(name, _TEMPLATE_WORKSPACE_NAME);
		}

		internal void ImportData(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			_importHelper.ImportData(workspaceArtifactId, documentsTestData);
		}

		internal void DeleteWorkspace(int artifactId)
		{
			using (var rsApiClient = Rsapi.CreateRsapiClient(ExecutionIdentity.System))
			{
				rsApiClient.Repositories.Workspace.DeleteSingle(artifactId);
			}
		}

		internal int CreateSavedSearch(FieldEntry[] defaultFields, FieldEntry[] additionalFields, int workspaceId, string savedSearchName)
		{
			var fields = defaultFields
				.Select(x => new FieldRef(x.DisplayName))
				.Concat(additionalFields.Select(x => new FieldRef(x.DisplayName)))
				.ToList();

			var folder = new SearchContainer
			{
				Name = _SAVED_SEARCH_FOLDER
			};
			var folderArtifactId = SavedSearch.CreateSearchFolder(workspaceId, folder);

			var search = new KeywordSearch
			{
				Name = savedSearchName,
				ArtifactTypeID = (int) ArtifactType.Document,
				SearchContainer = new SearchContainerRef(folderArtifactId),
				Fields = fields
			};
			return SavedSearch.Create(workspaceId, search);
		}

		internal int CreateSavedSearch(FieldEntry[] defaultFields, FieldEntry[] additionalFields, int workspaceId)
		{
			return CreateSavedSearch(defaultFields, additionalFields, workspaceId, _SAVED_SEARCH_NAME);
		}

		internal int CreateProduction(int workspaceArtifactId, int savedSearchId, string productionName)
		{
			var placeHolderFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory,
				@"TestData\DefaultPlaceholder.tif");

			var placeHolderFileData = FileToBase64Converter.Convert(placeHolderFilePath);

			var productionId = Production.Create(workspaceArtifactId, productionName);

			var placeholderId = Placeholder.Create(workspaceArtifactId, placeHolderFileData);
			ProductionDataSource.CreateDataSourceWithPlaceholder(workspaceArtifactId, productionId, savedSearchId, "WhenNoImageExists", placeholderId);

			Production.StageAndWaitForCompletion(workspaceArtifactId, productionId);
			Production.RunAndWaitForCompletion(workspaceArtifactId, productionId);

			return productionId;
		}

		internal int GetView(int workspaceId, string viewName)
		{
			return View.QueryView(workspaceId, viewName);
		}

		#endregion Methods
	}
}