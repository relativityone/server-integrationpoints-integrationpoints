using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Common
{
	using System;
	using System.Collections.Generic;

	public class IntegrationPointsAction
	{
		protected readonly RemoteWebDriver Driver;
		protected readonly TestContext Context;
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestContext));

		public IntegrationPointsAction(RemoteWebDriver driver, TestContext context)
		{
			Driver = driver;
			Context = context;
		}

		public ExportFirstPage SetupSyncFirstPage(RelativityProviderModel model)
		{
			GeneralPage generalPage = GoToWorkspacePage();
			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);
			return firstPage;
		}

		public IntegrationPointDetailsPage CreateNewExportEntityToLoadfileIntegrationPoint(
			EntityExportToLoadFileModel model)
		{
			GeneralPage generalPage = GoToWorkspacePage();

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			ExportEntityToFileSecondPage secondPage = SetupEntityExportToFileSecondPage(firstPage, model);

			ExportToFileThirdPage thirdPage = SetupExportToFileThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}

		public IntegrationPointDetailsPage CreateNewExportToLoadFileIntegrationPoint(
			ExportToLoadFileProviderModel model)
		{
			GeneralPage generalPage = GoToWorkspacePage();

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			ExportToFileSecondPage secondPage = SetupExportToFileSecondPage(firstPage, model);

			ExportToFileThirdPage thirdPage = SetupExportToFileThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}

		public IntegrationPointDetailsPage CreateNewRelativityProviderIntegrationPoint(RelativityProviderModel model)
		{
			GeneralPage generalPage = GoToWorkspacePage();

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			PushToRelativitySecondPage secondPage = SetupPushToRelativitySecondPage(firstPage, model);

			PushToRelativityThirdPage thirdPage = SetupPushToRelativityThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}

		public PushToRelativityThirdPage CreateNewRelativityProviderFieldMappingPage(RelativityProviderModel model)
		{
			GeneralPage generalPage = GoToWorkspacePage();

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			PushToRelativitySecondPage secondPage = SetupPushToRelativitySecondPage(firstPage, model);

			return secondPage.GoToNextPage();
		}

		public IntegrationPointDetailsPage CreateNewRelativityProviderIntegrationPointFromProfile(
			IntegrationPointGeneralModel model)
		{
			GeneralPage generalPage = GoToWorkspacePage();

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			return firstPage.SaveIntegrationPoint();
		}

		protected GeneralPage GoToWorkspacePage()
		{
			Log.Information("GoToWorkspacePage");
			return new GeneralPage(Driver).PassWelcomeScreen().ChooseWorkspace(Context.WorkspaceName);
		}

		protected ExportFirstPage ApplyModelToFirstPage(ExportFirstPage firstPage, IntegrationPointGeneralModel model)
		{
			firstPage.Name = model.Name;
			firstPage.Destination = model.DestinationProvider;
			if (!string.IsNullOrEmpty(model.TransferredObject))
			{
				firstPage.TransferedObject =
					CustodianToEntityUtils.GetValidTransferredObjectName(
						firstPage.IsEntityTransferredObjectOptionAvailable, model);
			}

			if (!string.IsNullOrEmpty(model.Profile))
			{
				firstPage.ProfileObject = model.Profile;
			}

			if (model.Scheduler.Enable)
			{
				firstPage.ToggleScheduler(model.Scheduler.Enable);
			}

			return firstPage;
		}

		public PushToRelativitySecondPage SetupPushToRelativitySecondPage(ExportFirstPage firstPage, RelativityProviderModel model)
		{
			PushToRelativitySecondPage secondPage = firstPage.GoToNextPagePush();
			Log.Information("secondPage");
			SelectSource(secondPage, model);

			secondPage.DestinationWorkspace = model.DestinationWorkspace;
			SelectDestination(secondPage, model);
			return secondPage;
		}

		public PushToRelativityThirdPage SetupPushToRelativityThirdPage(PushToRelativitySecondPage secondPage, RelativityProviderModel model)
		{
			PushToRelativityThirdPage thirdPage = secondPage.GoToNextPage();
			Log.Information("thirdPage");

			if (model.GetValueOrDefault(m => m.Source) != RelativityProviderModel.SourceTypeEnum.Production &&
			    model.GetValueOrDefault(m => m.Location) != RelativityProviderModel.LocationEnum.ProductionSet)
			{
				MapWorkspaceFields(thirdPage, model.FieldMapping);
				thirdPage.SelectCopyImages(model.CopyImages);
			}


			if (model.Overwrite == RelativityProviderModel.OverwriteModeEnum.AppendOnly)
			{
				thirdPage.SelectOverwrite = "Append Only";
			}
			else if (model.Overwrite == RelativityProviderModel.OverwriteModeEnum.OverlayOnly)
			{
				thirdPage.SelectOverwrite = "Overlay Only";
			}
			else if (model.Overwrite == RelativityProviderModel.OverwriteModeEnum.AppendOverlay)
			{
				thirdPage.SelectOverwrite = "Append/Overlay";
			}


			if (model.MultiSelectFieldOverlay ==
			    RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.MergeValues)
			{
				thirdPage.SelectMultiSelectFieldOverlayBehavior = "Merge Values";
			}
			else if (model.MultiSelectFieldOverlay ==
			         RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.ReplaceValues)
			{
				thirdPage.SelectMultiSelectFieldOverlayBehavior = "Replace Values";
			}
			else if (model.MultiSelectFieldOverlay ==
			         RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings)
			{
				thirdPage.SelectMultiSelectFieldOverlayBehavior = "Use Field Settings";
			}

			if (model.ImagePrecedence == ImagePrecedence.OriginalImages)
			{
				thirdPage.SelectImagePrecedence = "Original Images";
			}
			else if (model.ImagePrecedence == ImagePrecedence.ProducedImages)
			{
				thirdPage.SelectImagePrecedence = "Produced Images";
				thirdPage.SelectProductionPrecedence(model.SourceProductionName);
				thirdPage.SelectIncludeOriginalImagesIfNotProduced(model.IncludeOriginalImagesIfNotProduced);
			}

			thirdPage.SelectCopyNativeFiles(model.CopyNativeFiles);

			if (model.UseFolderPathInformation == RelativityProviderModel.UseFolderPathInformationEnum.No)
			{
				thirdPage.SelectFolderPathInfo = "No";
			}
			else if (model.UseFolderPathInformation ==
			         RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField)
			{
				thirdPage.SelectFolderPathInfo = "Read From Field";
				thirdPage.SelectReadFromField = "Document Folder Path";
			}
			else if (model.UseFolderPathInformation ==
			         RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree)
			{
				thirdPage.SelectFolderPathInfo = "Read From Folder Tree";
			}

			thirdPage.SelectMoveExitstingDocuments(model.MoveExistingDocuments);


			thirdPage.SelectCopyFilesToRepository(model.CopyFilesToRepository);

			return thirdPage;
		}
        public PushToRelativityThirdPage EditGoToFieldMappingPage(IntegrationPointDetailsPage detailsPage)
        {
            ExportFirstPage firstPage = detailsPage.EditIntegrationPoint();
            return firstPage.GoToNextPagePush().GoToNextPage();
        }

		private ExportFirstPage SetupFirstIntegrationPointPage(GeneralPage generalPage, IntegrationPointGeneralModel model)
		{
			ExportFirstPage firstPage = GoToFirstPageIntegrationPoints(generalPage);
			ExportFirstPage firstPageWithModelApplied = ApplyModelToFirstPage(firstPage, model);
			Log.Information("firstPageWithModelApplied");
			return firstPageWithModelApplied;
		}

		private ExportFirstPage GoToFirstPageIntegrationPoints(GeneralPage generalPage)
		{
			Log.Information("GoToFirstPageIntegrationPoints");
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			return ipPage.CreateNewExportIntegrationPoint();
		}

		private ExportToFileSecondPage SetupExportToFileSecondPage(ExportFirstPage firstPage,
			ExportToLoadFileProviderModel model)
		{
			ExportToFileSecondPage secondPage = firstPage.GoToNextPage();

			Log.Information("ExportToFileSecondPage");
			secondPage.Source = model.SourceInformationModel.Source;
			if (model.SourceInformationModel.Source == ExportToLoadFileSourceConstants.SAVED_SEARCH)
			{
				secondPage.SelectSavedSearch(model.SourceInformationModel.SavedSearch);
			}
			else if (model.SourceInformationModel.Source == ExportToLoadFileSourceConstants.PRODUCTION)
			{
				secondPage.ProductionSet = model.SourceInformationModel.ProductionSet;
			}
			else if (ExportToLoadFileSourceConstants.IsFolder(model.SourceInformationModel.Source))
			{
				secondPage.Folder = model.SourceInformationModel.Folder;
				secondPage.View = model.SourceInformationModel.View;
			}

			secondPage.StartExportAtRecord = model.SourceInformationModel.StartAtRecord;
			Thread.Sleep(200);
			if (model.SourceInformationModel.SelectAllFields)
			{
				secondPage.SelectAllSourceFields();
			}
			else
			{
				foreach (var field in model.SourceInformationModel.SelectedFields)
				{
					secondPage.SelectSourceField(field);
				}
			}
			Log.Information("secondPage");
			return secondPage;
			
		}

		private void SetupExportToFileThirdPageExportDetails(ExportToFileThirdPageExportDetails thirdPageExportDetails,
			ExportToLoadFileDetailsModel exportDetails)
		{
			if (exportDetails.ExportImages.HasValue && exportDetails.ExportImages.Value)
			{
				thirdPageExportDetails.SelectExportImages();
			}

			if (exportDetails.ExportNatives.HasValue && exportDetails.ExportNatives.Value)
			{
				thirdPageExportDetails.SelectExportNatives();
			}

			if (exportDetails.ExportTextFieldsAsFiles.HasValue && exportDetails.ExportTextFieldsAsFiles.Value)
			{
				thirdPageExportDetails.SelectExportTextFieldsAsFiles();
			}

			if (exportDetails.DestinationFolder == ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root)
			{
				thirdPageExportDetails.DestinationFolder.ChooseRootElement();
			}
			else if (exportDetails.DestinationFolder ==
			         ExportToLoadFileProviderModel.DestinationFolderTypeEnum.SubfolderOfRoot)
			{
				thirdPageExportDetails.DestinationFolder.ChooseFirstChildElement();
			}

			if (!exportDetails.CreateExportFolder.HasValue || !exportDetails.CreateExportFolder.Value)
			{
				thirdPageExportDetails.DeselectDoNotCreateExportFolder();
			}

			if (exportDetails.OverwriteFiles.HasValue && exportDetails.OverwriteFiles.Value)
			{
				thirdPageExportDetails.SelectOverwriteFiles();
			}
		}

		private void SetupExportToFileThirdPageLoadFileOptions(
			ExportToFileThirdPageLoadFileOptions thirdPageLoadFileOptions,
			ExportToLoadFileLoadFileOptionsModel loadFileOptions, bool exportNatives)
		{
			thirdPageLoadFileOptions.ImageFileFormat = loadFileOptions.ImageFileFormat;
			thirdPageLoadFileOptions.DataFileFormat = loadFileOptions.DataFileFormat;
			thirdPageLoadFileOptions.DataFileEncoding = loadFileOptions.DataFileEncoding;

			thirdPageLoadFileOptions.SelectFilePath(loadFileOptions.FilePathType);
			if (loadFileOptions.FilePathType == ExportToLoadFileProviderModel.FilePathTypeEnum.UserPrefix)
			{
				thirdPageLoadFileOptions.UserPrefix = loadFileOptions.UserPrefix;
			}

			if (!exportNatives && loadFileOptions.IncludeNativeFilesPath.HasValue &&
			    loadFileOptions.IncludeNativeFilesPath.Value)
			{
				thirdPageLoadFileOptions.IncludeNativeFilesPath();
			}

			if (loadFileOptions.ExportMultiChoiceAsNested.HasValue && loadFileOptions.ExportMultiChoiceAsNested.Value)
			{
				thirdPageLoadFileOptions.ExportMultipleChoiceFieldsAsNested();
			}

			thirdPageLoadFileOptions.NameOutputFilesAfter = loadFileOptions.NameOutputFilesAfter;

			if (loadFileOptions.AppendOriginalFileName.HasValue && loadFileOptions.AppendOriginalFileName.Value)
			{
				thirdPageLoadFileOptions.AppendOriginalFileName();
			}
		}

		private void SetupExportToFileThirdPageImageOptions(
			ExportToFileThirdPageImageNativeTextOptions thirdPageImageNativeTextOptions,
			ExportToLoadFileImageOptionsModel imageOptions)
		{
			thirdPageImageNativeTextOptions.FileType = imageOptions.ImageFileType;
			switch (imageOptions.ImagePrecedence)
			{
				case ImagePrecedence.OriginalImages:
					thirdPageImageNativeTextOptions.ImagePrecedence = "Original Images";
					break;
				case ImagePrecedence.ProducedImages:
					thirdPageImageNativeTextOptions.ImagePrecedence = "Produced Images";
					break;
				default:
					thirdPageImageNativeTextOptions.ImagePrecedence = "Original Images";
					break;
			}

			thirdPageImageNativeTextOptions.ImageSubdirectoryPrefix = imageOptions.ImageSubdirectoryPrefix;
		}

		private void SetupExportToFileThirdPageNativeOptions(
			ExportToFileThirdPageImageNativeTextOptions thirdPageImageNativeTextOptions,
			ExportToLoadFileNativeOptionsModel nativeOptions)
		{
			thirdPageImageNativeTextOptions.NativeSubdirectoryPrefix = nativeOptions.NativeSubdirectoryPrefix;
		}

		private void SetupExportToFileThirdPageTextOptions(
			ExportToFileThirdPageImageNativeTextOptions thirdPageImageNativeTextOptions,
			ExportToLoadFileTextOptionsModel textOptions)
		{
			thirdPageImageNativeTextOptions.TextFileEncoding = textOptions.TextFileEncoding;
			thirdPageImageNativeTextOptions.SelectTextPrecedenceField(textOptions.TextPrecedence);
			thirdPageImageNativeTextOptions.TextSubdirectoryPrefix = textOptions.TextSubdirectoryPrefix;
		}

		private void SetupExportToFileThirdPageVolumeSubdirectoryDetails(
			ExportToFileThirdPageVolumeSubdirectoryDetails thirdPageVolumeSubdirectoryDetails,
			ExportToLoadFileVolumeAndSubdirectoryModel volumeSubdirectoryDetails)
		{
			thirdPageVolumeSubdirectoryDetails.VolumePrefix = volumeSubdirectoryDetails.VolumePrefix;
			thirdPageVolumeSubdirectoryDetails.VolumeStartNumber =
				volumeSubdirectoryDetails.VolumeStartNumber?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.VolumeNumberOfDigits =
				volumeSubdirectoryDetails.VolumeNumberOfDigits?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.VolumeMaxSize =
				volumeSubdirectoryDetails.VolumeMaxSize?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.SubdirectoryStartNumber =
				volumeSubdirectoryDetails.SubdirectoryStartNumber?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.SubdirectoryNumberOfDigits =
				volumeSubdirectoryDetails.SubdirectoryNumberOfDigits?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.SubdirectoryMaxFiles =
				volumeSubdirectoryDetails.SubdirectoryMaxFiles?.ToString() ?? string.Empty;
		}

		private ExportToFileThirdPage SetupExportToFileThirdPage(ExportToFileSecondPage secondPage,
			ExportToLoadFileProviderModel model)
		{
			ExportToFileThirdPage thirdPage = secondPage.GoToNextPage();
			Log.Information("SetupExportToFileThirdPage");
			ExportToLoadFileDetailsModel exportDetails = model.ExportDetails;
			SetupExportToFileThirdPageExportDetails(thirdPage.ExportDetails, exportDetails);

			bool exportImages = exportDetails.ExportImages.GetValueOrDefault(false);
			bool exportNatives = exportDetails.ExportNatives.GetValueOrDefault(false);
			bool exportTextFieldsAsFiles = exportDetails.ExportTextFieldsAsFiles.GetValueOrDefault(false);
			bool volumeAndSubdirectoryDetailsVisible = exportImages || exportNatives || exportTextFieldsAsFiles;

			ExportToLoadFileLoadFileOptionsModel loadFileOptions = model.OutputSettings.LoadFileOptions;
			SetupExportToFileThirdPageLoadFileOptions(thirdPage.LoadFileOptions, loadFileOptions, exportNatives);

			if (exportImages)
			{
				ExportToLoadFileImageOptionsModel imageOptions = model.OutputSettings.ImageOptions;
				SetupExportToFileThirdPageImageOptions(thirdPage.ImageNativeTextOptions, imageOptions);
			}

			if (exportNatives)
			{
				ExportToLoadFileNativeOptionsModel nativeOptions = model.OutputSettings.NativeOptions;
				SetupExportToFileThirdPageNativeOptions(thirdPage.ImageNativeTextOptions, nativeOptions);
			}

			if (exportTextFieldsAsFiles)
			{
				ExportToLoadFileTextOptionsModel textOptions = model.OutputSettings.TextOptions;
				SetupExportToFileThirdPageTextOptions(thirdPage.ImageNativeTextOptions, textOptions);
			}

			if (volumeAndSubdirectoryDetailsVisible)
			{
				ExportToLoadFileVolumeAndSubdirectoryModel volumeSubdirectoryDetails =
					model.ToLoadFileVolumeAndSubdirectoryModel;
				SetupExportToFileThirdPageVolumeSubdirectoryDetails(thirdPage.VolumeSubdirectoryDetails,
					volumeSubdirectoryDetails);
			}
			Log.Information("return thirdPage");
			return thirdPage;
		}

		private ExportToFileThirdPage SetupExportToFileThirdPage(ExportEntityToFileSecondPage secondPage,
			EntityExportToLoadFileModel model)
		{
			ExportToFileThirdPage thirdPage = secondPage.GoToNextPage();

			ExportToLoadFileDetailsModel exportDetails = model.ExportDetails;
			SetupExportToFileThirdPageExportDetails(thirdPage.ExportDetails, exportDetails);

			bool exportImages = exportDetails.ExportImages.GetValueOrDefault(false);
			bool exportNatives = exportDetails.ExportNatives.GetValueOrDefault(false);
			bool exportTextFieldsAsFiles = exportDetails.ExportTextFieldsAsFiles.GetValueOrDefault(false);
			bool volumeAndSubdirectoryDetailsVisible = exportImages || exportNatives || exportTextFieldsAsFiles;

			ExportToLoadFileLoadFileOptionsModel loadFileOptions = model.OutputSettings.LoadFileOptions;
			SetupExportToFileThirdPageLoadFileOptions(thirdPage.LoadFileOptions, loadFileOptions, exportNatives);

			if (exportImages)
			{
				ExportToLoadFileImageOptionsModel imageOptions = model.OutputSettings.ImageOptions;
				SetupExportToFileThirdPageImageOptions(thirdPage.ImageNativeTextOptions, imageOptions);
			}

			if (exportNatives)
			{
				ExportToLoadFileNativeOptionsModel nativeOptions = model.OutputSettings.NativeOptions;
				SetupExportToFileThirdPageNativeOptions(thirdPage.ImageNativeTextOptions, nativeOptions);
			}

			if (exportTextFieldsAsFiles)
			{
				ExportToLoadFileTextOptionsModel textOptions = model.OutputSettings.TextOptions;
				SetupExportToFileThirdPageTextOptions(thirdPage.ImageNativeTextOptions, textOptions);
			}

			if (volumeAndSubdirectoryDetailsVisible)
			{
				ExportToLoadFileVolumeAndSubdirectoryModel volumeSubdirectoryDetails =
					model.OutputSettings.VolumeAndSubdirectoryOptions;
				SetupExportToFileThirdPageVolumeSubdirectoryDetails(thirdPage.VolumeSubdirectoryDetails,
					volumeSubdirectoryDetails);
			}

			return thirdPage;
		}

		private ExportEntityToFileSecondPage SetupEntityExportToFileSecondPage(ExportFirstPage firstPage,
			EntityExportToLoadFileModel model)
		{
			Log.Information("SetupEntityExportToFileSecondPage");
			ExportEntityToFileSecondPage secondPage = firstPage.GotoNextPageEntity();
			secondPage.View = model.ExportDetails.View;
			Thread.Sleep(500);
			if (model.ExportDetails.SelectAllFields)
			{
				secondPage.SelectAllFields();
			}
			Log.Information("END SetupEntityExportToFileSecondPage");
			return secondPage;
		}

		private static void SelectSource(PushToRelativitySecondPage secondPage, RelativityProviderModel model)
		{
			RelativityProviderModel.SourceTypeEnum sourceType = model.GetValueOrDefault(m => m.Source);

			if (model.Source.HasValue)
			{
				secondPage.SourceSelect = sourceType == RelativityProviderModel.SourceTypeEnum.SavedSearch
					? "Saved Search"
					: "Production";
			}

			switch (sourceType)
			{
				case RelativityProviderModel.SourceTypeEnum.SavedSearch:
					secondPage.SelectSavedSearch(model.GetValueOrDefault(m => m.SavedSearch));
					break;
				case RelativityProviderModel.SourceTypeEnum.Production:
					secondPage.SelectSourceProduction(model.SourceProductionName);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static void SelectDestination(PushToRelativitySecondPage secondPage, RelativityProviderModel model)
		{
			RelativityProviderModel.LocationEnum location = model.GetValueOrDefault(m => m.Location);

			switch (location)
			{
				case RelativityProviderModel.LocationEnum.Folder:
					secondPage.SelectFolderLocation();
					secondPage.WaitForPage();
					secondPage.FolderLocationSelect.ChooseRootElement();
					break;
				case RelativityProviderModel.LocationEnum.ProductionSet:
					secondPage.SelectProductionLocation(model.DestinationProductionName);
					break;
			}
		}

		public void MapWorkspaceFields(PushToRelativityThirdPage thirdPage, List<Tuple<string, string>> fieldMapping)
		{
			if (fieldMapping == null)
			{
				thirdPage.MapAllFields();
				return;
			}

			foreach (Tuple<string, string> tuple in fieldMapping)
			{
				thirdPage.SelectSourceField(tuple.Item1);
				thirdPage.SelectWorkspaceField(tuple.Item2);
			}
		}
	}
}