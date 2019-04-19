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
using kCura.WinEDDS.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using FieldRef = Relativity.Services.Field.FieldRef;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class WorkspaceService
	{
		private const string _TEMPLATE_WORKSPACE_NAME = "Relativity Starter Template";
		private const string _LEGACY_TEMPLATE_WORKSPACE_NAME = "kCura Starter Template";
		private const string _SAVED_SEARCH_FOLDER = "Testing Folder";

		internal const int _PRODUCTION_PLACEHOLDER_ARTIFACT_TYPE_ID = 1000035;

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

		public bool TryImportData(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			return _importHelper.ImportData(workspaceArtifactId, documentsTestData);
		}

		public void ImportData(int workspaceId, DocumentsTestData documentsTestData)
		{
			bool importSucceeded = TryImportData(workspaceId, documentsTestData);
			if (!importSucceeded)
			{
				throw new ImportIOException();
			}
		}

		public bool ImportSingleExtractedText(int workspaceArtifactId, string controlNumber, string extractedTextFilePath)
		{
			System.Data.DataTable documentData = 
				DocumentTestDataBuilder.GetSingleExtractedTextDocument(controlNumber, extractedTextFilePath);
			return _importHelper.ImportMetadataFromFileWithExtractedTextInFile(workspaceArtifactId, documentData);
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

		public async Task<int> GetProductionPlaceholderArtifactIDAsync(
			int workspaceArtifactID,
			IObjectManager objectManager)
		{
			QueryResult result = await objectManager.QueryAsync(workspaceArtifactID, new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						ArtifactTypeID = _PRODUCTION_PLACEHOLDER_ARTIFACT_TYPE_ID
					},
					Fields = new[]
					{
						new global::Relativity.Services.Objects.DataContracts.FieldRef
						{
							Name = "Default"
						}
					}
				}, 1, 1)
				.ConfigureAwait(false);
			return result.Objects.Single().ArtifactID;
		}

		public int GetView(int workspaceId, string viewName)
		{
			return View.QueryView(workspaceId, viewName);
		}
	}
}