using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.Productions.Services;
using Relativity.Services.Search;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using FieldRef = Relativity.Services.Field.FieldRef;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class WorkspaceService
	{
		private const string _SAVED_SEARCH_FOLDER = "Testing Folder";
		private readonly ImportHelper _importHelper;

		public const int PRODUCTION_MAX_RETRIES_COUNT = 100;

		public WorkspaceService(ImportHelper importHelper)
		{
			_importHelper = importHelper;
		}

		public int CreateWorkspace(string name)
		{
			string templateName = SharedVariables.UseLegacyTemplateName()
				? WorkspaceTemplateNames.LEGACY_TEMPLATE_WORKSPACE_NAME
				: WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME;
			return Workspace.CreateWorkspace(name, templateName);
		}

		public void ImportData(int workspaceID, DocumentsTestData documentsTestData)
		{
			bool importSucceeded = _importHelper.ImportData(workspaceID, documentsTestData);
			if (!importSucceeded)
			{
				string errorsDetails = _importHelper.ErrorMessages.Any() 
					? $" Error messages: {string.Join("; ", _importHelper.ErrorMessages)}"
					: " No error messages.";
				throw new TestException("Importing documents does not succeeded." + errorsDetails);
			}
		}

		public bool ImportSingleExtractedText(int workspaceArtifactID, string controlNumber, string extractedTextFilePath)
		{
			System.Data.DataTable documentData =
				DocumentTestDataBuilder.GetSingleExtractedTextDocument(controlNumber, extractedTextFilePath);
			return _importHelper.ImportMetadataFromFileWithExtractedTextInFile(workspaceArtifactID, documentData);
		}


		public void DeleteWorkspace(int artifactID)
		{
			using (IRSAPIClient rsapiClient = Rsapi.CreateRsapiClient())
			{
				rsapiClient.Repositories.Workspace.DeleteSingle(artifactID);
			}
		}

		public int CreateSavedSearch(FieldEntry[] fieldEntries, int workspaceID, string savedSearchName)
		{
			List<FieldRef> fields = fieldEntries
				.Select(x => new FieldRef(x.DisplayName))
				.ToList();

			return CreateSavedSearch(fields, workspaceID, savedSearchName);
		}

		public int CreateSavedSearch(IEnumerable<string> fields, int workspaceID, string savedSearchName)
		{
			return CreateSavedSearch(fields.Select(displayName => new FieldRef(displayName)).ToList(), workspaceID,
				savedSearchName);
		}

		public int CreateSavedSearch(List<FieldRef> fields, int workspaceID, string savedSearchName)
		{
			var folder = new SearchContainer
			{
				Name = _SAVED_SEARCH_FOLDER
			};
			int folderArtifactID = SavedSearch.CreateSearchFolder(workspaceID, folder);

			var search = new KeywordSearch
			{
				Name = savedSearchName,
				ArtifactTypeID = (int)ArtifactType.Document,
				SearchContainer = new SearchContainerRef(folderArtifactID),
				Fields = fields
			};
			return SavedSearch.Create(workspaceID, search);
		}

		public int CreateProductionSet(int workspaceArtifactID, string productionSetName)
		{
			return CreateProductionSetAsync(workspaceArtifactID, productionSetName).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public Task<int> CreateProductionSetAsync(int workspaceArtifactID, string productionSetName)
		{
			return Production.Create(workspaceArtifactID, productionSetName);
		}

		public ProductionCreateResultDto CreateAndRunProduction(
			int workspaceArtifactID, 
			int savedSearchID, 
			string productionName, 
			ProductionType productionType, 
			int retriesCount = PRODUCTION_MAX_RETRIES_COUNT)
		{
			string placeHolderFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\DefaultPlaceholder.tif");

			return CreateAndRunProduction(
				workspaceArtifactID, 
				savedSearchID, 
				productionName, 
				placeHolderFilePath,
				productionType, 
				retriesCount);
		}

		public ProductionCreateResultDto CreateAndRunProduction(
			int workspaceArtifactID, 
			int savedSearchID,
			string productionName, 
			string placeHolderFilePath, 
			ProductionType productionType,
			int retriesCount = PRODUCTION_MAX_RETRIES_COUNT)
		{
			return CreateAndRunProductionAsync(
				workspaceArtifactID,
				savedSearchID,
				productionName,
				placeHolderFilePath,
				productionType,
				retriesCount
			).GetAwaiter().GetResult();
		}

		public async Task<ProductionCreateResultDto> CreateAndRunProductionAsync(
			int workspaceArtifactID, 
			int savedSearchID,
			string productionName, 
			string placeHolderFilePath, 
			ProductionType productionType,
			int retriesCount)
		{
			byte[] placeHolderFileDataBytes = File.ReadAllBytes(placeHolderFilePath);
			int productionSetArtifactID = CreateProductionSet(workspaceArtifactID, productionName);
			int placeholderID = Placeholder.Create(workspaceArtifactID, placeHolderFileDataBytes);

			int dataSourceArtifactID = await ProductionDataSource.CreateDataSourceWithPlaceholderAsync(
				workspaceArtifactID,
				productionSetArtifactID,
				savedSearchID,
				productionType,
				UseImagePlaceholderOption.WhenNoImageExists,
				placeholderID
			).ConfigureAwait(false);

			await StageProductionAsync(workspaceArtifactID, productionName, productionSetArtifactID, retriesCount).ConfigureAwait(false);
			await RunProductionAsync(workspaceArtifactID, productionName, productionSetArtifactID, retriesCount).ConfigureAwait(false);

			return new ProductionCreateResultDto(productionSetArtifactID, dataSourceArtifactID);
		}

		public int GetView(int workspaceID, string viewName)
		{
			return View.QueryView(workspaceID, viewName);
		}

		private static async Task StageProductionAsync(int workspaceArtifactID, string productionName, int productionSetArtifactID, int retriesCount)
		{
			bool wasStagedSuccessfully = await Production.StageAndWaitForCompletionAsync(
				workspaceArtifactID,
				productionSetArtifactID,
				retriesCount
			).ConfigureAwait(false);
			if (!wasStagedSuccessfully)
			{
				throw new TestException($"Error occured while staging production: {productionName}");
			}
		}

		private static async Task RunProductionAsync(int workspaceArtifactID, string productionName, int productionSetArtifactID, int retriesCount)
		{
			bool wasRanSuccessfully = await Production.RunAndWaitForCompletionAsync(
				workspaceArtifactID,
				productionSetArtifactID,
				retriesCount
			).ConfigureAwait(false);
			if (!wasRanSuccessfully)
			{
				throw new TestException($"Error occured while running production: {productionName}");
			}
		}
	}
}