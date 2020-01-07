﻿using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoint.Tests.Core.Models.Import;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.Documents;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions.Images;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions.Productions;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Documents;
using kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Images;
using kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Productions;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using System.IO;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.ImportFromLoadFile
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.WEB_IMPORT_EXPORT)]
	[Category(TestCategory.IMPORT_FROM_LOAD_FILE)]
	[Category(TestCategory.NOT_WORKING_ON_TRIDENT)] //Remove when DEVOPS-70257
	public class ImportFromLoadFileTest : UiTest
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Install(SourceContext.GetWorkspaceId());
			CopyFilesToFileshare();
		}

		private void CopyFilesToFileshare()
		{
			string sourceLocation = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataImportFromLoadFile");

			if (SharedVariables.UiUseTapiForFileCopy)
			{
				const int tapiTimeoutInSeconds = 60 * 3;
				FileCopier.UploadToImportDirectory(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataImportFromLoadFile"),
					SharedVariables.RelativityBaseAdressUrlValue,
					SourceContext.GetWorkspaceId(),
					SharedVariables.RelativityUserName,
					SharedVariables.RelativityPassword,
					tapiTimeoutInSeconds);
			}
			else
			{
				string workspaceFolderName = $"EDDS{SourceContext.GetWorkspaceId()}";
				string destinationLocation =
					Path.Combine(SharedVariables.FileshareLocation, workspaceFolderName, "DataTransfer", "Import");
				FileCopier.CopyDirectory(sourceLocation, destinationLocation);
			}
		}
		
		[IdentifiedTest("85d418ab-7bf2-4047-b810-88e0551a1e27")]
		[RetryOnError]
		[Order(10)]
		public void ImportDocumentsFromLoadFile()
		{
			// Arrange
			var model = new ImportDocumentsFromLoadFileModel($"Import Documents from load file ({Now})", TransferredObjectConstants.DOCUMENT)
			{
				LoadFileSettings =
				{
					ImportType = ImportType.DocumentLoadFile,
					WorkspaceDestinationFolder = SourceContext.WorkspaceName,
					ImportSource = @"Small Salt.dat",
					StartLine = 0
				},
				FileEncoding = FileEncodingModel.CreateDefault(),
				FieldsMapping = new FieldsMappingModel(
					"Control Number", "Control Number [Object Identifier]",
					"Extracted Text", "Extracted Text [Long Text]"
				),
				Settings = new SettingsModel
				{
					Overwrite = OverwriteType.AppendOverlay,
					MultiSelectFieldOverlayBehavior = MultiSelectFieldOverlayBehavior.UseFieldSettings,
					CopyNativeFiles = CopyNativeFiles.PhysicalFiles,
					NativeFilePath = "FILE_PATH",
					UseFolderPathInformation = true,
					FolderPathInformation = "Folder Path",
					MoveExistingDocuments = false,
					CellContainsFileLocation = true,
					CellContainingFileLocation = "Extracted Text",
					EncodingForUndetectableFiles = LoadFileEncodingConstants.UTF_8,
				}
			};

			// Act
			new ImportDocumentsFromLoadFileActions(Driver, SourceContext, model).Setup();

			var detailsPage = new IntegrationPointDetailsPage(Driver);
			detailsPage.RunIntegrationPoint();

			// Assert
			new BaseUiValidator().ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
		
		[IdentifiedTest("b3ef81f0-f020-462e-94ce-be9e645b389a")]
		[RetryOnError]
		[Order(20)]
		[Ignore("REL-291039, it fails when whole fixture is executed, passes when first test is skipped.")]
		public void ImportImagesFromLoadFile()
		{
			// Arrange
			var model = new ImportImagesFromLoadFileModel($"Import Images from load file ({Now})", TransferredObjectConstants.DOCUMENT)
			{
				LoadFileSettings =
				{
					ImportType = ImportType.ImageLoadFile,
					WorkspaceDestinationFolder = SourceContext.WorkspaceName,
					ImportSource = @"Small Salt Images.opt",
					StartLine = 0
				},
				ImportSettings =
				{
					Numbering = Numbering.AutoNumberPages,
					ImportMode = OverwriteType.AppendOverlay,
					CopyFilesToDocumentRepository = true,
					LoadExtractedText = false
				}
			};

			// Act
			new ImportImagesFromLoadFileActions(Driver, SourceContext, model).Setup();

			var detailsPage = new IntegrationPointDetailsPage(Driver);
			detailsPage.RunIntegrationPoint();

			// Assert
			new BaseUiValidator().ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[IdentifiedTest("efc71b8b-9f6e-421d-8dff-e9453eb9ab53")]
		[RetryOnError]
		[Order(30)]
		[Ignore("REL-291039 it fails when whole fixture is executed, passes when first test is skipped.")]
		public void ImportProductionsFromLoadFile()
		{
			// Arrange
			string productionSetName = $"Production set {Now}";
			SourceContext.CreateProductionSet(productionSetName);

			var model = new ImportProductionsFromLoadFileModel($"Import Productions from load file ({Now})", TransferredObjectConstants.DOCUMENT)
			{
				LoadFileSettings =
				{
					ImportType = ImportType.ProductionLoadFile,
					WorkspaceDestinationFolder = SourceContext.WorkspaceName,
					ImportSource = @"Small Salt Productions.opt",
					StartLine = 0
				},
				ImportSettings =
				{
					Numbering = Numbering.AutoNumberPages,
					ImportMode = OverwriteType.AppendOverlay,
					CopyFilesToDocumentRepository = true,
					Production = productionSetName
				}
			};

			// Act
			new ImportProductionsFromLoadFileActions(Driver, SourceContext, model).Setup();

			var detailsPage = new IntegrationPointDetailsPage(Driver);
			detailsPage.RunIntegrationPoint();

			// Assert
			new BaseUiValidator().ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

	}
}
