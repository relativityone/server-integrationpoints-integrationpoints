using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using IntegrationPointType = kCura.IntegrationPoint.Tests.Core.Models.IntegrationPointType;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{
	[TestFixture]
	[Category(TestCategory.EXPORT_TO_LOAD_FILE)]
    public class FolderExportToLoadFileTests : ExportToLoadFileTests
	{
		private IntegrationPointsAction _integrationPointsAction;

		[SetUp]
		public void SetUp()
		{
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}

		[Test, Order(1)]
		public void FolderExportToLoadFile_TC_ELF_DIR_1()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_DIR_1");

			// Step 1
			model.Type = IntegrationPointType.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = TransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.FOLDER;
			model.SourceInformationModel.Folder = "One";
			model.SourceInformationModel.View = "Documents";
			model.SourceInformationModel.StartAtRecord = 1;
			model.SourceInformationModel.SelectAllFields = true;
			
			// Step 3
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = true;
			model.ExportDetails.ExportNatives = true;
			model.ExportDetails.ExportTextFieldsAsFiles = true;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;
			model.ExportDetails.CreateExportFolder = true;
			model.ExportDetails.OverwriteFiles = true;

			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.OPTICON;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.DAT;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = LoadFileEncodingConstants.UTF_8;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Relative;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = false;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = false;

			model.OutputSettings.ImageOptions.ImageFileType = ExportToLoadFileImageFileTypeConstants.SINGLE_PAGE_TIFF_JPEG;
			model.OutputSettings.ImageOptions.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.OutputSettings.ImageOptions.ImageSubdirectoryPrefix = "IMG";

			model.OutputSettings.NativeOptions.NativeSubdirectoryPrefix = "NATIVE";

			model.OutputSettings.TextOptions.TextFileEncoding = LoadFileEncodingConstants.UTF_8;
			model.OutputSettings.TextOptions.TextPrecedence = "Extracted Text";
			model.OutputSettings.TextOptions.TextSubdirectoryPrefix = "TEXT";

			model.OutputSettings.VolumeAndSubdirectoryOptions.VolumePrefix = "VOL";
			model.OutputSettings.VolumeAndSubdirectoryOptions.VolumeStartNumber = 1;
			model.OutputSettings.VolumeAndSubdirectoryOptions.VolumeNumberOfDigits = 4;
			model.OutputSettings.VolumeAndSubdirectoryOptions.VolumeMaxSize = 4400;

			model.OutputSettings.VolumeAndSubdirectoryOptions.SubdirectoryStartNumber = 1;
			model.OutputSettings.VolumeAndSubdirectoryOptions.SubdirectoryNumberOfDigits = 4;
			model.OutputSettings.VolumeAndSubdirectoryOptions.SubdirectoryMaxFiles = 500;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadFileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[Test, Order(2)]
		public void FolderExportToLoadFile_TC_ELF_DIR_2()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_DIR_2");

			// Step 1
			model.Type = IntegrationPointType.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = TransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.FOLDER_AND_SUBFOLDERS;
			model.SourceInformationModel.Folder = "One";
			model.SourceInformationModel.View = "Documents";
			model.SourceInformationModel.StartAtRecord = 5;
			model.SourceInformationModel.SelectAllFields = true;

			// Step 3
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = true;
			model.ExportDetails.ExportNatives = true;
			model.ExportDetails.ExportTextFieldsAsFiles = true;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;
			model.ExportDetails.CreateExportFolder = true;
			model.ExportDetails.OverwriteFiles = true;

			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.OPTICON;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.DAT;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = LoadFileEncodingConstants.UTF_8;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Relative;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = false;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = false;

			model.OutputSettings.ImageOptions.ImageFileType = ExportToLoadFileImageFileTypeConstants.SINGLE_PAGE_TIFF_JPEG;
			model.OutputSettings.ImageOptions.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.OutputSettings.ImageOptions.ImageSubdirectoryPrefix = "IMAGE_FILES";

			model.OutputSettings.NativeOptions.NativeSubdirectoryPrefix = "NATIVE_FILES";

			model.OutputSettings.TextOptions.TextFileEncoding = LoadFileEncodingConstants.UTF_8;
			model.OutputSettings.TextOptions.TextPrecedence = "Extracted Text";
			model.OutputSettings.TextOptions.TextSubdirectoryPrefix = "TEXT_FILES";

			model.OutputSettings.VolumeAndSubdirectoryOptions.VolumePrefix = "VOLUME";
			model.OutputSettings.VolumeAndSubdirectoryOptions.VolumeStartNumber = 1;
			model.OutputSettings.VolumeAndSubdirectoryOptions.VolumeNumberOfDigits = 4;
			model.OutputSettings.VolumeAndSubdirectoryOptions.VolumeMaxSize = 4400;

			model.OutputSettings.VolumeAndSubdirectoryOptions.SubdirectoryStartNumber = 1;
			model.OutputSettings.VolumeAndSubdirectoryOptions.SubdirectoryNumberOfDigits = 4;
			model.OutputSettings.VolumeAndSubdirectoryOptions.SubdirectoryMaxFiles = 500;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadFileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}
