using System;
using System.IO;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Actions;
using kCura.IntegrationPoints.UITests.Auxiliary;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.ImportFromLoadFile
{
	public class ImportFromLoadFileTest : UiTest
	{
		private int _workspaceId;
		private IntegrationPointsImportFromLoadFileAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_workspaceId = Context.GetWorkspaceId();
			_integrationPointsAction = new IntegrationPointsImportFromLoadFileAction(Driver, Context);
			Install(_workspaceId);
			CopyFilesToFileshare();
		}

		private void CopyFilesToFileshare()
		{
			string fileshareLocation = SharedVariables.FileshareLocation;
			string workspaceFolderName = $"EDDS{_workspaceId}";
			string sourceLocation = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataImportFromLoadFile");
			string destinationLocation = Path.Combine(fileshareLocation, workspaceFolderName, "DataTransfer", "Import");
			FileCopyHelper.CopyDirectory(sourceLocation, destinationLocation);
		}

		[Test, Order(1)]
		public void DocumentImportFromLoadFile_TC_ILF_DOC_1()
		{
			// Arrange
			var model = new ImportFromLoadFileModel("TC_ILF_DOC_1", ExportToLoadFileTransferredObjectConstants.DOCUMENT);

			model.LoadFileSettings.ImportType = ImportType.DocumentLoadFile;
			model.LoadFileSettings.WorkspaceDestinationFolder = "Three";
			model.LoadFileSettings.ImportSource = "ExampleLoadFile.txt";
			model.LoadFileSettings.StartLine = 0;

			model.FileEncoding.FileEncoding = LoadFileEncodingConstants.UNICODE;
			model.FileEncoding.Column = 12;
			model.FileEncoding.Quote = 13;
			model.FileEncoding.Newline = 14;
			model.FileEncoding.MultiValue = 15;
			model.FileEncoding.NestedValue = 16;

			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("test", "Control Number"));
			model.SharedImportSettings.MapFieldsAutomatically = false;
			model.SharedImportSettings.Overwrite = OverwriteType.AppendOverlay;

			model.ImportDocumentSettings.CopyNativeFiles = CopyNativeFiles.LinksOnly;
			model.ImportDocumentSettings.NativeFilePath = "test";
			model.ImportDocumentSettings.UseFolderPathInformation = true;
			model.ImportDocumentSettings.FolderPathInformation = "test";
			model.ImportDocumentSettings.CellContainsFileLocation = true;
			model.ImportDocumentSettings.FileLocationCell = "Select...";
			model.ImportDocumentSettings.EncodingForUndetectableFiles = LoadFileEncodingConstants.UTF_8;

			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewImportFromLoadFileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// TODO this is only an example test to validate page objects
		}
	}
}
