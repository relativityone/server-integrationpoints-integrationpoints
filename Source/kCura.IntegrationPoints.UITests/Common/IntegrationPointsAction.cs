using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Common
{
	using System;
	using System.Collections.Generic;

	public class IntegrationPointsAction
	{
		private readonly RemoteWebDriver _driver;
		private readonly TestContext _context;

		public IntegrationPointsAction(RemoteWebDriver driver, TestContext context)
		{
			_driver = driver;
			_context = context;
		}


		public ExportFirstPage SetupFirstIntegrationPointPage(GeneralPage generalPage, IntegrationPointGeneralModel model)
		{
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage firstPage = ipPage.CreateNewIntegrationPoint();
			firstPage.Name = model.Name;
			firstPage.Destination = model.DestinationProvider;

			return firstPage;
		}

		public ExportToFileSecondPage SetupExportToFileSecondPage(ExportFirstPage firstPage,
			ExportToLoadFileProviderModel model)
		{
			ExportToFileSecondPage secondPage = firstPage.GoToNextPage();

			secondPage.SelectAllDocuments();

			return secondPage;
		}

		public ExportToFileThirdPage SetupExportToFileThirdPage(ExportToFileSecondPage secondPage,
			ExportToLoadFileProviderModel model)
		{
			ExportToFileThirdPage thirdPage = secondPage.GoToNextPage();

			thirdPage.DestinationFolder.ChooseRootElement();

			return thirdPage;
		}

		public IntegrationPointDetailsPage CreateNewExportToLoadfileIntegrationPoint(ExportToLoadFileProviderModel model)
		{
			var generalPage = new GeneralPage(_driver);
			generalPage.ChooseWorkspace(_context.WorkspaceName);

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			ExportToFileSecondPage secondPage = SetupExportToFileSecondPage(firstPage, model);

			ExportToFileThirdPage thirdPage = SetupExportToFileThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}

		public IntegrationPointDetailsPage CreateNewRelativityProviderIntegrationPoint(RelativityProviderModel model)
		{
			var generalPage = new GeneralPage(_driver);
			generalPage.ChooseWorkspace(_context.WorkspaceName);

			ExportFirstPage firstPage = SetupFirstIntegrationPointPage(generalPage, model);

			PushToRelativitySecondPage secondPage = SetupPushToRelativitySecondPage(firstPage, model);

			PushToRelativityThirdPage thirdPage = SetupPushToRelativityThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
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

			thirdPage.SelectCopyImages( model.CopyImages );

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

			if (model.ImagePrecedence == RelativityProviderModel.ImagePrecedenceEnum.OriginalImages)
			{
				thirdPage.SelectImagePrecedence = "Original Images";
			}
			else if (model.ImagePrecedence == RelativityProviderModel.ImagePrecedenceEnum.ProducedImages)
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
