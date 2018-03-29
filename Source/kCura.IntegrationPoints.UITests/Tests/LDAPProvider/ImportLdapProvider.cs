

using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Ldap;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.LDAPProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class ImportLdapProvider : UiTest

	{
		[Test, Order(1)]
		public void DocumentExportToLoadFile_TC_ELF_DOC_1()
		{
			// Arrange
			//ImportFromLdapModel model = CreateExportToLoadFileProviderModel("TC_ELF_DOC_1");

			//// Step 1
			//model.Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Export;
			//model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			//model.TransferredObject = ExportToLoadFileTransferredObjectConstants.DOCUMENT;

			//// Step 2
			//model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.SAVED_SEARCH;
			//model.SourceInformationModel.StartAtRecord = 1;
			//model.SourceInformationModel.SelectAllFields = true;

			//// Step 3
			//model.ExportDetails.LoadFile = true;
			//model.ExportDetails.ExportImages = true;
			//model.ExportDetails.ExportNatives = true;
			//model.ExportDetails.ExportTextFieldsAsFiles = true;
			//model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;
			//model.ExportDetails.CreateExportFolder = true;
			//model.ExportDetails.OverwriteFiles = true;

			//model.OutputSettings.LoadFileOptions.ImageFileFormat = ExportToLoadFileImageFileFormatConstants.OPTICON;
			//model.OutputSettings.LoadFileOptions.DataFileFormat = ExportToLoadFileDataFileFormatConstants.DAT;
			//model.OutputSettings.LoadFileOptions.DataFileEncoding = ExportToLoadFileFileEncodingConstants.UTF_8;
			//model.OutputSettings.LoadFileOptions.FilePathType = ExportToLoadFileProviderModel.FilePathTypeEnum.Relative;
			//model.OutputSettings.LoadFileOptions.IncludeNativeFilesPath = true;
			//model.OutputSettings.LoadFileOptions.ExportMultiChoiceAsNested = false;
			//model.OutputSettings.LoadFileOptions.NameOutputFilesAfter = ExportToLoadFileNameOutputFilesAfterConstants.IDENTIFIER;
			//model.OutputSettings.LoadFileOptions.AppendOriginalFileName = false;

			//model.OutputSettings.ImageOptions.ImageFileType = ExportToLoadFileImageFileTypeConstants.MULTI_PAGE_TIFF_JPEG;
			//model.OutputSettings.ImageOptions.ImageSubdirectoryPrefix = "IMG";

			//model.OutputSettings.NativeOptions.NativeSubdirectoryPrefix = "NATIVE";

			//model.OutputSettings.TextOptions.TextFileEncoding = ExportToLoadFileFileEncodingConstants.UTF_8;
			//model.OutputSettings.TextOptions.TextSubdirectoryPrefix = "TEXT";

			//model.ToLoadFileVolumeAndSubdirectoryModel.VolumePrefix = "VOL";
			//model.ToLoadFileVolumeAndSubdirectoryModel.VolumeStartNumber = 1;
			//model.ToLoadFileVolumeAndSubdirectoryModel.VolumeNumberOfDigits = 4;
			//model.ToLoadFileVolumeAndSubdirectoryModel.VolumeMaxSize = 4400;

			//model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryStartNumber = 1;
			//model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryNumberOfDigits = 4;
			//model.ToLoadFileVolumeAndSubdirectoryModel.SubdirectoryMaxFiles = 500;

			//var validator = new ExportToLoadFileProviderValidator();

			//// Act
			//IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadfileIntegrationPoint(model);
			//detailsPage.RunIntegrationPoint();

			//// Assert
			//validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}
