using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class DocumentExportToLoadFileTests : ExportToLoadFileTests
	{
		private IntegrationPointsAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}

		[Test, Order(1)]
		public void DocumentExportToLoadFile_TC_ELF_DOC_1()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_DOC_1");

			// Step 1
			model.Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = ExportToLoadFileTransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.SAVED_SEARCH;
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
			model.OutputSettings.LoadFileOptions.DataFileEncoding = ExportToLoadFileFileEncodingConstants.UTF_8;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Relative;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = false;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = false;

			model.OutputSettings.ImageOptions.ImageFileType = ExportToLoadFileImageFileTypeConstants.MULTI_PAGE_TIFF_JPEG;
			model.OutputSettings.ImageOptions.ImageSubdirectoryPrefix = "IMG";

			model.OutputSettings.NativeOptions.NativeSubdirectoryPrefix = "NATIVE";

			model.OutputSettings.TextOptions.TextFileEncoding = ExportToLoadFileFileEncodingConstants.UTF_8;
			model.OutputSettings.TextOptions.TextSubdirectoryPrefix = "TEXT";

			model.ToLoadFileVolumeAndSubdirectoryModel.VolumePrefix = "VOL";
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeStartNumber = 1;
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeMaxSize = 4400;

			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryStartNumber = 1;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryMaxFiles = 500;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadfileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
		
		[Test, Order(2)]
		public void DocumentExportToLoadFile_TC_ELF_DOC_3()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_DOC_3");

			// Step 1
			model.Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = ExportToLoadFileTransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.SAVED_SEARCH;
			model.SourceInformationModel.StartAtRecord = 1;
			model.SourceInformationModel.SelectAllFields = true;

			// Step 3
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = false;
			model.ExportDetails.ExportNatives = false;
			model.ExportDetails.ExportTextFieldsAsFiles = false;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;
			model.ExportDetails.CreateExportFolder = true;
			model.ExportDetails.OverwriteFiles = true;

			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.NO_IMAGE_LOAD_FILE;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.CUSTOM;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = ExportToLoadFileFileEncodingConstants.UNICODE;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Absolute;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = false;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = true;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = true;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadfileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[Test, Order(3)]
		public void DocumentExportToLoadFile_TC_ELF_DOC_5()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_DOC_5");

			// Step 1
			//default

			// Step 2
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = true;
			model.ExportDetails.ExportNatives = true;
			model.ExportDetails.ExportTextFieldsAsFiles = true;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.SubfolderOfRoot;
			model.ExportDetails.CreateExportFolder = false;
			model.ExportDetails.OverwriteFiles = true;

			// Step 3
			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.IPRO_FULL_TEXT;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.CSV;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = ExportToLoadFileFileEncodingConstants.WESTERN_EUROPEAN_WINDOWS;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = true;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = true;

			model.OutputSettings.ImageOptions.ImageFileType = ExportToLoadFileImageFileTypeConstants.PDF;
			model.OutputSettings.TextOptions.TextFileEncoding = ExportToLoadFileFileEncodingConstants.WESTERN_EUROPEAN_WINDOWS;

			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryNumberOfDigits = 4;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadfileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[Test, Order(4)]
		public void DocumentExportToLoadFile_TC_ELF_DOC_2()
		{
			// Arrange
			Context.ImportDocuments(true, DocumentTestDataBuilder.TestDataType.SaltPepperWithFolderStructure);
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_DOC_2");

			// Step 1
			//default
			model.SourceInformationModel.StartAtRecord = 100;
			model.SourceInformationModel.Source = "Saved Search";
			model.SourceInformationModel.SavedSearch = "All Documents";

			// Step 2
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = true;
			model.ExportDetails.ExportNatives = true;
			model.ExportDetails.ExportTextFieldsAsFiles = true;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;
			model.ExportDetails.CreateExportFolder = false;
			model.ExportDetails.OverwriteFiles = true;

			// Step 3
			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.IPRO;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.CSV;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = ExportToLoadFileFileEncodingConstants.WESTERN_EUROPEAN_WINDOWS;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Relative;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = false;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = true;

			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = "Identifier";
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = true;

			model.OutputSettings.ImageOptions.ImageFileType = ExportToLoadFileImageFileTypeConstants.PDF;
			model.OutputSettings.ImageOptions.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.OutputSettings.ImageOptions.ImageSubdirectoryPrefix = "IMAGE_FILES";

			model.OutputSettings.NativeOptions.NativeSubdirectoryPrefix = "NATIVE_FILES";

			model.OutputSettings.TextOptions.TextFileEncoding = ExportToLoadFileFileEncodingConstants.UNICODE;
			model.OutputSettings.TextOptions.TextPrecedence = "Extracted Text";

			model.ToLoadFileVolumeAndSubdirectoryModel.VolumePrefix = "VOLUME";
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeStartNumber = 1;
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeMaxSize = 4400;

			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryStartNumber = 1;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryMaxFiles = 500;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadfileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[Test, Order(5)]
		public void DocumentExportToLoadFile_TC_ELF_DOC_4()
		{
			// Arrange
			// Data is imported in DocumentExportToLoadFile_TC_ELF_DOC_2
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_DOC_4");

			// Step 1
			model.Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = ExportToLoadFileTransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.SAVED_SEARCH;
			model.SourceInformationModel.StartAtRecord = 1;
			model.SourceInformationModel.SelectAllFields = false;
			// control number is exported by default

			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportNatives = true;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.SubfolderOfRoot;

			// Step 3
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = false;
			model.ExportDetails.ExportNatives = true;
			model.ExportDetails.ExportTextFieldsAsFiles = false;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.SubfolderOfRoot;
			model.ExportDetails.CreateExportFolder = true;
			model.ExportDetails.OverwriteFiles = false;

			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.NO_IMAGE_LOAD_FILE;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.DAT;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = ExportToLoadFileFileEncodingConstants.UTF_8;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Relative;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = false;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = true;

			model.OutputSettings.NativeOptions.NativeSubdirectoryPrefix = "NATIVE";

			model.ToLoadFileVolumeAndSubdirectoryModel.VolumePrefix = "VOL";
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeStartNumber = 1;
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeMaxSize = 10;

			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryStartNumber = 1;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryMaxFiles = 500;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadfileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}
