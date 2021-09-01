using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services;
using NUnit.Framework;
using Relativity.DataExchange.Service;
using Relativity.IntegrationPoints.Contracts.Models;
using Directory = kCura.Utility.Directory;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals
{
	internal class ExportTestContextProvider
	{
		private readonly WorkspaceService _workspaceService;
		private readonly IExportFieldsService _fieldsService;

		private readonly ExportTestContext _testContext;
		private readonly ExportTestConfiguration _testConfiguration;

		private readonly FolderWithDocumentsIdRetriever _folderIDRetriever;

		public ExportTestContextProvider(
			WorkspaceService workspaceService,
			IExportFieldsService fieldsService,
			ExportTestContext testContext,
			ExportTestConfiguration testConfiguration,
			FolderWithDocumentsIdRetriever folderIdRetriever)
		{
			_workspaceService = workspaceService;
			_fieldsService = fieldsService;

			_testContext = testContext;
			_testConfiguration = testConfiguration;

			_folderIDRetriever = folderIdRetriever;
		}

		public async Task InitializeContextAsync()
		{
			CreateTestWorkspace();
			CreateAndImportTestData();
			InstallDataTransferLegacy();

			AddWorkspaceFieldsToContext();
			CreateSavedSearch();
			AddViewIdToContext();
			await CreateAndImportIntoProductionAsync().ConfigureAwait(false);

			Directory.Instance.CreateDirectoryIfNotExist(_testConfiguration.DestinationPath);
		}

		public void DeleteContext()
		{
			Directory.Instance.DeleteDirectoryIfExists(
				_testConfiguration.DestinationPath, 
				recursive: true, 
				exceptOnExistenceCheckError: false);

			if (_testContext.WorkspaceID > 0)
			{
				_workspaceService.DeleteWorkspace(_testContext.WorkspaceID);
			}
		}

		private void CreateTestWorkspace()
		{
			_testContext.WorkspaceID = _workspaceService.CreateWorkspace(_testContext.WorkspaceName);
		}

		private void CreateAndImportTestData()
		{
			_testContext.DocumentsTestData = DocumentTestDataBuilder.BuildTestData();
			_workspaceService.ImportData(_testContext.WorkspaceID, _testContext.DocumentsTestData);
			_folderIDRetriever.UpdateFolderIdsAsync(_testContext.WorkspaceID, _testContext.DocumentsTestData.Documents)
				.GetAwaiter().GetResult();
		}

		private void InstallDataTransferLegacy()
		{
			RelativityApplicationManager appManager = new RelativityApplicationManager(new TestHelper());
			appManager.ImportApplicationToLibraryAsync(Path.Combine(TestContext.Parameters["BuildToolsDirectory"], SharedVariables.DataTransferLegacyRapPath)).GetAwaiter().GetResult();
		}

		private void AddWorkspaceFieldsToContext()
		{
			FieldEntry[] fields = _fieldsService.GetAllExportableFields(_testContext.WorkspaceID, (int)ArtifactType.Document);

			_testContext.DefaultFields = fields.OrderBy(x => x.DisplayName).ToArray();
			_testContext.LongTextField = fields.FirstOrDefault(x => x.DisplayName == _testConfiguration.LongTextFieldName);
		}

		private void CreateSavedSearch()
		{
			_testContext.ExportedObjArtifactID = _workspaceService.CreateSavedSearch(
				_testContext.DefaultFields,
				_testContext.WorkspaceID,
				_testConfiguration.SavedSearchArtifactName);
		}

		private void AddViewIdToContext()
		{
			_testContext.ViewID = _workspaceService.GetView(_testContext.WorkspaceID, _testConfiguration.ViewName);
		}

		private async Task CreateAndImportIntoProductionAsync()
		{
			_testContext.ProductionArtifactID = await _workspaceService.CreateProductionAsync(_testContext.WorkspaceID, _testConfiguration.ProductionArtifactName).ConfigureAwait(false);

			DocumentsTestData productionData = DocumentTestDataBuilder.BuildTestData();
			_workspaceService.ImportDataToProduction(_testContext.WorkspaceID, _testContext.ProductionArtifactID, productionData.Images);
		}
	}
}
