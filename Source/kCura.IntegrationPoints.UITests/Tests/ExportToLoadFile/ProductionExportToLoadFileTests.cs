using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
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
    public class ProductionExportToLoadFileTests : ExportToLoadFileTests
	{
		private string PRODUCTION_NAME_SMALL = "Small Production under tests";
		private string PRODUCTION_NAME_BIG = "Big Production under tests";
		private IntegrationPointsAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Context.CreateAndRunProduction(SAVED_SEARCH_NAME, PRODUCTION_NAME_SMALL);
			Context.CreateAndRunProduction(SAVED_SEARCH_NAME, PRODUCTION_NAME_BIG);
		}

		[SetUp]
		public void SetUp()
		{
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}

		[Test]
		[Ignore("TODO - create JIRA, defect: The given key was not present in the dictionary.")]
		public void ProductionExportToLoadFile_TC_ELF_PROD_1()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_PROD_1");

			// Step 1
			model.Type = IntegrationPointType.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = TransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.PRODUCTION;
			model.SourceInformationModel.ProductionSet = PRODUCTION_NAME_SMALL;
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

			model.OutputSettings.ImageOptions.ImageFileType = ExportToLoadFileImageFileTypeConstants.MULTI_PAGE_TIFF_JPEG;
			model.OutputSettings.ImageOptions.ImageSubdirectoryPrefix = "IMG";

			model.OutputSettings.NativeOptions.NativeSubdirectoryPrefix = "NATIVE";

			model.OutputSettings.TextOptions.TextFileEncoding = LoadFileEncodingConstants.UTF_8;
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
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadFileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[Test]
		[Ignore("1) REL-258301 2) TODO - create JIRA, defect: The given key was not present in the dictionary.")]
		public void ProductionExportToLoadFile_TC_ELF_PROD_2()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_PROD_2");

			// Step 1
			model.Type = IntegrationPointType.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = TransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.PRODUCTION;
			model.SourceInformationModel.ProductionSet = PRODUCTION_NAME_BIG;
			model.SourceInformationModel.StartAtRecord = 10;
			model.SourceInformationModel.SelectAllFields = false;
			// Production::Begin Bates, Production::End Bates are selected by default

			// Step 3
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = false;
			model.ExportDetails.ExportNatives = false;
			model.ExportDetails.ExportTextFieldsAsFiles = false;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;
			model.ExportDetails.CreateExportFolder = false;
			model.ExportDetails.OverwriteFiles = true;

			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.NO_IMAGE_LOAD_FILE;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.CSV;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = LoadFileEncodingConstants.WESTERN_EUROPEAN_WINDOWS;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Absolute;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = true;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = true;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadFileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[Test]
		[Ignore("TODO - create JIRA, defect: The given key was not present in the dictionary.")]
		public void ProductionExportToLoadFile_TC_ELF_PROD_3()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_PROD_3");

			// Step 1
			model.Type = IntegrationPointType.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = TransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.PRODUCTION;
			model.SourceInformationModel.ProductionSet = PRODUCTION_NAME_SMALL;
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
			model.OutputSettings.LoadFileOptions.DataFileEncoding = LoadFileEncodingConstants.UNICODE;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Absolute;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = false;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = true;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = true;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadFileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[Test]
		[Ignore("TODO - create JIRA, defect: The given key was not present in the dictionary.")]
		public void ProductionExportToLoadFile_TC_ELF_PROD_4()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_PROD_4");

			// Step 1
			model.Type = IntegrationPointType.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = TransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.PRODUCTION;
			model.SourceInformationModel.ProductionSet = PRODUCTION_NAME_BIG;
			model.SourceInformationModel.StartAtRecord = 1;
			model.SourceInformationModel.SelectAllFields = false;
			model.SourceInformationModel.SelectedFields = new List<string> {"Control Number"};

			// Step 3
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = false;
			model.ExportDetails.ExportNatives = true;
			model.ExportDetails.ExportTextFieldsAsFiles = false;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;
			model.ExportDetails.CreateExportFolder = true;
			model.ExportDetails.OverwriteFiles = false;

			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.NO_IMAGE_LOAD_FILE;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.DAT;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = LoadFileEncodingConstants.UTF_8;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Relative;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = false;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = false;

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
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadFileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		[Test]
		[Ignore("TODO - create JIRA, defect: The given key was not present in the dictionary.")]
		public void ProductionExportToLoadFile_TC_ELF_PROD_5()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_PROD_5");

			// Step 1
			model.Type = IntegrationPointType.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = TransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.PRODUCTION;
			model.SourceInformationModel.ProductionSet = PRODUCTION_NAME_SMALL;
			model.SourceInformationModel.StartAtRecord = 1;
			model.SourceInformationModel.SelectAllFields = true;

			// Step 3
			model.ExportDetails.LoadFile = true;
			model.ExportDetails.ExportImages = true;
			model.ExportDetails.ExportNatives = true;
			model.ExportDetails.ExportTextFieldsAsFiles = true;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;
			model.ExportDetails.CreateExportFolder = false;
			model.ExportDetails.OverwriteFiles = true;

			model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.IPRO;
			model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.CSV;
			model.OutputSettings.LoadFileOptions.DataFileEncoding = LoadFileEncodingConstants.WESTERN_EUROPEAN_WINDOWS;
			model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Relative;
			model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = true;
			model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			model.OutputSettings.LoadFileOptions.AppendOriginalFileName = true;

			model.OutputSettings.ImageOptions.ImageFileType = ExportToLoadFileImageFileTypeConstants.PDF;
			model.OutputSettings.ImageOptions.ImageSubdirectoryPrefix = "IMAGE_FILES";

			model.OutputSettings.NativeOptions.NativeSubdirectoryPrefix = "NATIVE_FILES";

			model.OutputSettings.TextOptions.TextFileEncoding = LoadFileEncodingConstants.UNICODE;
			model.OutputSettings.TextOptions.TextSubdirectoryPrefix = "TEXT_FILES";

			model.ToLoadFileVolumeAndSubdirectoryModel.VolumePrefix = "VOLUME";
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeStartNumber = 1;
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.VolumeMaxSize = 4400;

			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryStartNumber = 1;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryNumberOfDigits = 4;
			model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryMaxFiles = 500;

			var validator = new ExportToLoadFileProviderValidator();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadFileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}
