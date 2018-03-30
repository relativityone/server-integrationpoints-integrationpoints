using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Common
{
	using System;
	using System.Collections.Generic;

	public class IntegrationPointsAction
	{
		protected readonly RemoteWebDriver Driver;
		protected readonly TestContext Context;

		public IntegrationPointsAction(RemoteWebDriver driver, TestContext context)
		{
			Driver = driver;
			Context = context;
		}


		public ExportFirstPage SetupFirstIntegrationPointPage(GeneralPage generalPage, IntegrationPointGeneralModel model)
		{
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage firstPage = ipPage.CreateNewExportIntegrationPoint();
			firstPage.Name = model.Name;
			firstPage.Destination = model.DestinationProvider;

			return firstPage;
		}

		public ExportToFileSecondPage SetupExportToFileSecondPage(ExportFirstPage firstPage,
			ExportToLoadFileProviderModel model)
		{
			ExportToFileSecondPage secondPage = firstPage.GoToNextPage();

			secondPage.SelectSavedSearch(model.SourceInformationModel.SavedSearch);

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

			return secondPage;
		}

		private void SetupExportToFileThirdPageExportDetails(ExportToFileThirdPageExportDetails thirdPageExportDetails, ExportToLoadFileDetailsModel exportDetails)
		{
			if (exportDetails.ExportImages.HasValue && exportDetails.ExportImages.Value)
			{
				thirdPageExportDetails.SelectExportImages();
			}

			if (exportDetails.ExportNatives.HasValue && exportDetails.ExportNatives.Value)
			{
				thirdPageExportDetails.SelectExportNatives();
			}

			if (exportDetails.ExportTextFieldsAsFiles.HasValue && exportDetails.ExportImages.Value)
			{
				thirdPageExportDetails.SelectExportTextFieldsAsFiles();
			}

			if (exportDetails.DestinationFolder == ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root)
			{
				thirdPageExportDetails.DestinationFolder.ChooseRootElement();
			}
			else if (exportDetails.DestinationFolder == ExportToLoadFileProviderModel.DestinationFolderTypeEnum.SubfolderOfRoot)
			{
				thirdPageExportDetails.DestinationFolder.ChooseFirstChildElement();
			}

			if (!exportDetails.CreateExportFolder.HasValue && exportDetails.CreateExportFolder.Value)
			{
				thirdPageExportDetails.DeselectDoNotCreateExportFolder();
			}

			if (exportDetails.OverwriteFiles.HasValue && exportDetails.OverwriteFiles.Value)
			{
				thirdPageExportDetails.SelectOverwriteFiles();
			}
		}

		private void SetupExportToFileThirdPageLoadFileOptions(ExportToFileThirdPageLoadFileOptions thirdPageLoadFileOptions, ExportToLoadFileLoadFileOptionsModel loadFileOptions, bool exportNatives)
		{
			thirdPageLoadFileOptions.ImageFileFormat = loadFileOptions.ImageFileFormat;
			thirdPageLoadFileOptions.DataFileFormat = loadFileOptions.DataFileFormat;
			thirdPageLoadFileOptions.DataFileEncoding = loadFileOptions.DataFileEncoding;

			thirdPageLoadFileOptions.SelectFilePath(loadFileOptions.FilePathType);
			if (loadFileOptions.FilePathType == ExportToLoadFileProviderModel.FilePathTypeEnum.UserPrefix)
			{
				thirdPageLoadFileOptions.UserPrefix = loadFileOptions.UserPrefix;
			}

			if (!exportNatives && loadFileOptions.IncludeNativeFilesPath.HasValue && loadFileOptions.IncludeNativeFilesPath.Value)
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

		private void SetupExportToFileThirdPageImageOptions(ExportToFileThirdPageImageNativeTextOptions thirdPageImageNativeTextOptions, ExportToLoadFileImageOptionsModel imageOptions)
		{
			thirdPageImageNativeTextOptions.FileType = imageOptions.ImageFileType;
			switch (imageOptions.ImagePrecedence)
			{
				case ImagePrecedenceEnum.OriginalImages:
					thirdPageImageNativeTextOptions.ImagePrecedence = "Original Images";
					break;
				case ImagePrecedenceEnum.ProducedImages:
					thirdPageImageNativeTextOptions.ImagePrecedence = "Produced Images";
					break;
				default:
					thirdPageImageNativeTextOptions.ImagePrecedence = "Original Images";
					break;
			}
			thirdPageImageNativeTextOptions.ImageSubdirectoryPrefix = imageOptions.ImageSubdirectoryPrefix;
		}

		private void SetupExportToFileThirdPageNativeOptions(ExportToFileThirdPageImageNativeTextOptions thirdPageImageNativeTextOptions, ExportToLoadFileNativeOptionsModel nativeOptions)
		{
			thirdPageImageNativeTextOptions.NativeSubdirectoryPrefix = nativeOptions.NativeSubdirectoryPrefix;
		}

		private void SetupExportToFileThirdPageTextOptions(ExportToFileThirdPageImageNativeTextOptions thirdPageImageNativeTextOptions, ExportToLoadFileTextOptionsModel textOptions)
		{
			thirdPageImageNativeTextOptions.TextFileEncoding = textOptions.TextFileEncoding;
			thirdPageImageNativeTextOptions.SelectTextPrecedenceField(textOptions.TextPrecedence);
			thirdPageImageNativeTextOptions.TextSubdirectoryPrefix = textOptions.TextSubdirectoryPrefix;
		}

		private void SetupExportToFileThirdPageVolumeSubdirectoryDetails(ExportToFileThirdPageVolumeSubdirectoryDetails thirdPageVolumeSubdirectoryDetails, ExportToLoadFileVolumeAndSubdirectoryModel volumeSubdirectoryDetails)
		{
			thirdPageVolumeSubdirectoryDetails.VolumePrefix = volumeSubdirectoryDetails.VolumePrefix;
			thirdPageVolumeSubdirectoryDetails.VolumeStartNumber = volumeSubdirectoryDetails.VolumeStartNumber?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.VolumeNumberOfDigits = volumeSubdirectoryDetails.VolumeNumberOfDigits?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.VolumeMaxSize = volumeSubdirectoryDetails.VolumeMaxSize?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.SubdirectoryStartNumber = volumeSubdirectoryDetails.SubdirectoryStartNumber?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.SubdirectoryNumberOfDigits = volumeSubdirectoryDetails.SubdirectoryNumberOfDigits?.ToString() ?? string.Empty;
			thirdPageVolumeSubdirectoryDetails.SubdirectoryMaxFiles = volumeSubdirectoryDetails.SubdirectoryMaxFiles?.ToString() ?? string.Empty;
		}

		public ExportToFileThirdPage SetupExportToFileThirdPage(ExportToFileSecondPage secondPage, ExportToLoadFileProviderModel model)
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
				ExportToLoadFileVolumeAndSubdirectoryModel volumeSubdirectoryDetails = model.ToLoadFileVolumeAndSubdirectoryModel;
				SetupExportToFileThirdPageVolumeSubdirectoryDetails(thirdPage.VolumeSubdirectoryDetails, volumeSubdirectoryDetails);
			}

			return thirdPage;
		}


		public IntegrationPointDetailsPage CreateNewExportToLoadfileIntegrationPoint(ExportToLoadFileProviderModel model)
		{
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			ExportToFileSecondPage secondPage = SetupExportToFileSecondPage(firstPage, model);

			ExportToFileThirdPage thirdPage = SetupExportToFileThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}

		public IntegrationPointDetailsPage CreateNewRelativityProviderIntegrationPoint(RelativityProviderModel model)
		{
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			PushToRelativitySecondPage secondPage = SetupPushToRelativitySecondPage(firstPage, model);

			PushToRelativityThirdPage thirdPage = SetupPushToRelativityThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}

		protected TImportFirstPage SetupImportFirstPage<TImportFirstPage, TImportSecondPage, TModel>(GeneralPage generalPage, IntegrationPointGeneralModel model,
			Func<TImportFirstPage> funcFirstPageCreator)
			where TImportSecondPage : ImportSecondBasePage<TModel>
			where TImportFirstPage : ImportFirstPage<TImportSecondPage, TModel>
		{
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			TImportFirstPage firstPage = ipPage.CreateNewImportIntegrationPoint<TImportFirstPage, TImportSecondPage, TModel>(funcFirstPageCreator);
			firstPage.Name = model.Name;
			firstPage.SelectImport();
			firstPage.Source = model.SourceProvider;
			firstPage.TransferredObject = model.TransferredObject;
			return firstPage;
		}

		protected TSecondPage SetupImportSecondPage<TSecondPage, TModel>(ImportFirstPage<TSecondPage, TModel> firstPage, TModel model)
			where TSecondPage : ImportSecondBasePage<TModel>
		{
			TSecondPage secondPage =  firstPage.GoToNextPage();
			secondPage.SetupModel(model);
			return secondPage;
		}

		protected ImportThirdPage<TModel> SetupImportThirdPage<TModel>(ImportSecondBasePage<TModel> secondPage, TModel model, Func<ImportThirdPage<TModel>> funcThridPageCreator)
		{
			ImportThirdPage<TModel> thirdPage = secondPage.GoToNextPage(funcThridPageCreator);
			return thirdPage;
		}

		public PushToRelativitySecondPage SetupPushToRelativitySecondPage(ExportFirstPage firstPage, RelativityProviderModel model)
		{
			PushToRelativitySecondPage secondPage = firstPage.GoToNextPagePush();
			secondPage.SelectAllDocuments();

			//secondPage.SourceSelect = model.SourceProvider;
			//secondPage.RelativityInstance = model.RelativityInstance;
			secondPage.DestinationWorkspace = model.DestinationWorkspace;
			Thread.Sleep(500);
			secondPage.SelectFolderLocation();
			secondPage.FolderLocationSelect.ChooseRootElement();

			return secondPage;
		}

		public PushToRelativityThirdPage SetupPushToRelativityThirdPage(PushToRelativitySecondPage secondPage, RelativityProviderModel model)
		{
			PushToRelativityThirdPage thirdPage = secondPage.GoToNextPage();

			MapWorkspaceFields(thirdPage, model.FieldMapping);

			thirdPage.SelectCopyImages(model.CopyImages);

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

			thirdPage.SelectCopyImages(model.CopyImages);

			if (model.ImagePrecedence == ImagePrecedenceEnum.OriginalImages)
			{
				thirdPage.SelectImagePrecedence = "Original Images";
			}
			else if (model.ImagePrecedence == ImagePrecedenceEnum.ProducedImages)
			{
				thirdPage.SelectImagePrecedence = "Produced Images";
			}

			thirdPage.SelectCopyNativeFiles(model.CopyNativeFiles);

			if (model.UseFolderPathInformation == RelativityProviderModel.UseFolderPathInformationEnum.No)
			{
				thirdPage.SelectFolderPathInfo = "No";
			}
			else if (model.UseFolderPathInformation == RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField)
			{
				thirdPage.SelectFolderPathInfo = "Read From Field";
				thirdPage.SelectReadFromField = "Document Folder Path";
			}
			else if (model.UseFolderPathInformation == RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree)
			{
				thirdPage.SelectFolderPathInfo = "Read From Folder Tree";
			}

			thirdPage.SelectMoveExitstingDocuments(model.MoveExistingDocuments);


			thirdPage.SelectCopyFilesToRepository(model.CopyFilesToRepository);

			return thirdPage;
		}

		private void MapWorkspaceFields(PushToRelativityThirdPage thirdPage, List<Tuple<string, string>> fieldMapping)
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
