using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.Relativity.Client;
using Relativity.Services.Search;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using FieldRef = Relativity.Services.Field.FieldRef;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class WorkspaceService
	{
		private const string _SAVED_SEARCH_FOLDER = "Testing Folder";
		private readonly ImportHelper _importHelper;
		private readonly ITestHelper _testHelper;

		public WorkspaceService(ImportHelper importHelper)
		{
			_importHelper = importHelper;
			_testHelper = new TestHelper();
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

		public void ImportDataToProduction(int workspaceID, int productionID, DataTable testData)
		{
			bool success = _importHelper.ImportToProductionSet(workspaceID, productionID, testData);
			if (!success)
			{
				string errorsDetails = _importHelper.ErrorMessages.Any() ? $"Error messages: {string.Join("; ", _importHelper.ErrorMessages)}" : "No error messages.";
				throw new TestException("Importing documents to production failed. " + errorsDetails);
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

		public async Task<int> CreateProductionAsync(int workspaceID, string productionName)
		{
			using (var objectManager = _testHelper.CreateProxy<IObjectManager>())
			{
				CreateResult createResult = await objectManager.CreateAsync(workspaceID, new CreateRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						Name = "Production"
					},
					FieldValues = new[]
					{
						new FieldRefValuePair()
						{
							Field = new global::Relativity.Services.Objects.DataContracts.FieldRef()
							{
								Name = "Name"
							},
							Value = productionName
						}
					}
				}).ConfigureAwait(false);
				if (createResult.EventHandlerStatuses.All(x => x.Success))
				{
					return createResult.Object.ArtifactID;
				}
				else
				{
					string errorMessages = string.Join(System.Environment.NewLine, createResult.EventHandlerStatuses.Select(x => x.Message));
					throw new TestException($"Cannot create production '{productionName}' in workspace {workspaceID}: {errorMessages}");
				}
			}
		}

		public async Task DeleteProductionAsync(int workspaceID, int productionID)
		{
			using (var objectManager = _testHelper.CreateProxy<IObjectManager>())
			{
				await objectManager.DeleteAsync(workspaceID, new DeleteRequest()
				{
					Object = new RelativityObjectRef()
					{
						ArtifactID = productionID
					}
				}).ConfigureAwait(false);
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

		private int CreateSavedSearch(List<FieldRef> fields, int workspaceID, string savedSearchName)
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

		public int GetView(int workspaceID, string viewName)
		{
			return View.QueryView(workspaceID, viewName);
		}
	}
}