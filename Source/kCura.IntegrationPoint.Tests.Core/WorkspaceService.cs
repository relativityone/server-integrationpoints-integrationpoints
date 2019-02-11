using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.Productions.Services;
using Relativity.Services.Field;
using Relativity.Services.Search;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class WorkspaceService
	{
		private const string _TEMPLATE_WORKSPACE_NAME = "Relativity Starter Template";
		private const string _LEGACY_TEMPLATE_WORKSPACE_NAME = "kCura Starter Template";
		private const string _SAVED_SEARCH_FOLDER = "Testing Folder";

		private readonly ImportHelper _importHelper;

		public WorkspaceService(ImportHelper importHelper)
		{
			_importHelper = importHelper;
		}

		public static string StarterTemplateName => SharedVariables.UseLegacyTemplateName()
			? _LEGACY_TEMPLATE_WORKSPACE_NAME
			: _TEMPLATE_WORKSPACE_NAME;

		public int CreateWorkspace(string name, string template = null)
		{
			string templateName = template ?? StarterTemplateName;
			return Workspace.CreateWorkspace(name, templateName);
		}

		public bool ImportData(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			return _importHelper.ImportData(workspaceArtifactId, documentsTestData);
		}

		public bool ImportExtractedTextSimple(int workspaceArtifactId, string controlNumber, string extractedTextFilePath)
		{
			System.Data.DataTable documentData = 
				DocumentTestDataBuilder.GetSingleExtractedTextDocument(controlNumber, extractedTextFilePath);
			return _importHelper.ImportExtractedTextSimple(workspaceArtifactId, documentData);
		}


		public void DeleteWorkspace(int artifactId)
		{
			using (IRSAPIClient rsapiClient = Rsapi.CreateRsapiClient())
			{
				rsapiClient.Repositories.Workspace.DeleteSingle(artifactId);
			}
		}

		public int CreateSavedSearch(FieldEntry[] fieldEntries,  int workspaceId, string savedSearchName)
		{
			List<FieldRef> fields = fieldEntries
				.Select(x => new FieldRef(x.DisplayName))
				.ToList();

			return CreateSavedSearch(fields, workspaceId, savedSearchName);
		}

		public int CreateSavedSearch(IEnumerable<string> fields, int workspaceId, string savedSearchName)
		{
			return CreateSavedSearch(fields.Select(displayName => new FieldRef(displayName)).ToList(), workspaceId,
				savedSearchName);
		}

		public int CreateSavedSearch(List<FieldRef> fields, int workspaceId, string savedSearchName)
		{
			var folder = new SearchContainer
			{
				Name = _SAVED_SEARCH_FOLDER
			};
			int folderArtifactId = SavedSearch.CreateSearchFolder(workspaceId, folder);

			var search = new KeywordSearch
			{
				Name = savedSearchName,
				ArtifactTypeID = (int)ArtifactType.Document,
				SearchContainer = new SearchContainerRef(folderArtifactId),
				Fields = fields
			};
			return SavedSearch.Create(workspaceId, search);
		}

		public int CreateProductionSet(int workspaceArtifactId, string productionSetName)
		{
			return CreateProductionSetAsync(workspaceArtifactId, productionSetName).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public async Task<int> CreateProductionSetAsync(int workspaceArtifactId, string productionSetName)
		{
			return await Production.Create(workspaceArtifactId, productionSetName);
		}

		public int CreateAndRunProduction(int workspaceArtifactId, int savedSearchId, string productionName)
		{
			string placeHolderFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\DefaultPlaceholder.tif");

			return CreateAndRunProduction(workspaceArtifactId, savedSearchId, productionName, placeHolderFilePath);
		}

		public int CreateAndRunProduction(int workspaceArtifactId, int savedSearchId, string productionName, string placeHolderFilePath)
		{
			return CreateAndRunProductionAsync(
					workspaceArtifactId,
					savedSearchId,
					productionName,
					placeHolderFilePath
				).GetAwaiter().GetResult();
		}

		public async Task<int> CreateAndRunProductionAsync(int workspaceArtifactId, int savedSearchId, string productionName, string placeHolderFilePath)
		{
			byte[] placeHolderFileDataBytes = File.ReadAllBytes(placeHolderFilePath);
			int productionId = CreateProductionSet(workspaceArtifactId, productionName);
			int placeholderId = Placeholder.Create(workspaceArtifactId, placeHolderFileDataBytes);

			await ProductionDataSource.CreateDataSourceWithPlaceholderAsync(
				workspaceArtifactId,
				productionId,
				savedSearchId,
				UseImagePlaceholderOption.WhenNoImageExists,
				placeholderId
			).ConfigureAwait(false);

			await Production.StageAndWaitForCompletionAsync(
				workspaceArtifactId,
				productionId
			).ConfigureAwait(false);

			await Production.RunAndWaitForCompletionAsync(
				workspaceArtifactId,
				productionId
			).ConfigureAwait(false);

			return productionId;
		}

		public int GetView(int workspaceId, string viewName)
		{
			return View.QueryView(workspaceId, viewName);
		}
	}
}