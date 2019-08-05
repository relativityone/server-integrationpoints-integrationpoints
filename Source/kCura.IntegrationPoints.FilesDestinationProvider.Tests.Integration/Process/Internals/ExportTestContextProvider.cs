﻿using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.Utility;
using Relativity.DataExchange.Service;

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

		public void InitializeContext()
		{
			CreateTestWorkspace();
			CreateAndImportTestData();

			AddWorkspaceFieldsToContext();
			CreateSavedSearch();
			AddViewIdToContext();
			CreateAndRunProduction();

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
			_workspaceService.TryImportData(_testContext.WorkspaceID, _testContext.DocumentsTestData);
			_folderIDRetriever.UpdateFolderIdsAsync(_testContext.WorkspaceID, _testContext.DocumentsTestData.Documents)
				.GetAwaiter().GetResult();
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

		private void CreateAndRunProduction()
		{
			ProductionCreateResultDto productionCreateResult = _workspaceService.CreateAndRunProduction(
				_testContext.WorkspaceID,
				_testContext.ExportedObjArtifactID,
				_testConfiguration.ProductionArtifactName);

			_testContext.ProductionArtifactID = productionCreateResult.ProductionArtifactID;
		}
	}
}