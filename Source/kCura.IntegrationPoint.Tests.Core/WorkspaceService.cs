using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class WorkspaceService
	{
		#region Constructors

		public WorkspaceService(ImportHelper importHelper)
		{
			_importHelper = importHelper;
		}

		#endregion //Constructors

		#region Fields

		private readonly ImportHelper _importHelper;

		private const string _TEMPLATE_WORKSPACE_NAME = "Relativity Starter Template";
		private const string _SAVED_SEARCH_FOLDER = "Testing Folder";
		private const string _SAVED_SEARCH_NAME = "Testing Saved Search";

		#endregion //Fields

		#region Methods

		public int CreateWorkspace(string name)
		{
			return Workspace.CreateWorkspace(name, _TEMPLATE_WORKSPACE_NAME);
		}

		public void ImportData(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			_importHelper.ImportData(workspaceArtifactId, documentsTestData);
		}

		public void DeleteWorkspace(int artifactId)
		{
			using (var rsApiClient = Rsapi.CreateRsapiClient(ExecutionIdentity.System))
			{
				rsApiClient.Repositories.Workspace.DeleteSingle(artifactId);
			}
		}

		public int CreateSavedSearch(FieldEntry[] defaultFields, FieldEntry[] additionalFields, int workspaceId, string savedSearchName)
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
				ArtifactTypeID = (int)ArtifactType.Document,
				SearchContainer = new SearchContainerRef(folderArtifactId),
				Fields = fields
			};
			return SavedSearch.Create(workspaceId, search);
		}

		public int CreateSavedSearch(FieldEntry[] defaultFields, FieldEntry[] additionalFields, int workspaceId)
		{
			return CreateSavedSearch(defaultFields, additionalFields, workspaceId, _SAVED_SEARCH_NAME);
		}

		public int CreateProduction(int workspaceArtifactId, int savedSearchId, string productionName)
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

		public int GetView(int workspaceId, string viewName)
		{
			return View.QueryView(workspaceId, viewName);
		}

		#endregion Methods
	}
}