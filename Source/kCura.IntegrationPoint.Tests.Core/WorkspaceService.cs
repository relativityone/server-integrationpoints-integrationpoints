using kCura.IntegrationPoint.Tests.Core.Models;
using Relativity.Services.Search;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Productions.Services;
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
			return Workspace.CreateWorkspaceAsync(name, templateName).GetAwaiter().GetResult().ArtifactID;
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

        public void DeleteWorkspace(int artifactID)
		{
			Workspace.DeleteWorkspaceAsync(artifactID).GetAwaiter().GetResult();
		}

		public async Task<int> CreateProductionAsync(int workspaceID, string productionName)
		{
			using (var productionManager = _testHelper.CreateProxy<IProductionManager>())
			{
				var production = new Production
				{
					Name = productionName,
					ShouldCopyInstanceOnWorkspaceCreate = false,
					Details = new ProductionDetails
					{
						BrandingFontSize = 10,
						ScaleBrandingFont = false
					},
					Numbering = new DocumentFieldNumbering
					{
						NumberingType = NumberingType.DocumentField,
						NumberingField = new FieldRef
						{
							ArtifactID = 1003667,
							ViewFieldID = 0,
							Name = "Control Number"
						},
						AttachmentRelationalField = new FieldRef
						{
							ArtifactID = 0,
							ViewFieldID = 0,
							Name = ""
						},
						BatesPrefix = "PRE",
						BatesSuffix = "SUF",
						IncludePageNumbers = false,
						DocumentNumberPageNumberSeparator = "",
						NumberOfDigitsForPageNumbering = 0,
						StartNumberingOnSecondPage = false
					}
				};

				return await productionManager.CreateSingleAsync(workspaceID, production).ConfigureAwait(false);
			}
		}

		public int CreateSavedSearch(FieldEntry[] fieldEntries, int workspaceID, string savedSearchName)
		{
			List<FieldRef> fields = fieldEntries
				.Select(x => new FieldRef(x.DisplayName))
				.ToList();

			return CreateSavedSearch(fields, workspaceID, savedSearchName);
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
