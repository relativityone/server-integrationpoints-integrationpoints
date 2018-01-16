using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Common
{
	public class IntegrationPointsAction
	{
		private readonly RemoteWebDriver _driver;
		private readonly TestContext _context;

		public IntegrationPointsAction(RemoteWebDriver driver, TestContext context)
		{
			_driver = driver;
			_context = context;
		}


		public ExportFirstPage SetupFirstIntegrationPointsPage(GeneralPage generalPage, ExportToLoadFileProviderModel model)
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

		public IntegrationPointDetailsPage CreateNewExportToLoadfileIntegrationPointAction(ExportToLoadFileProviderModel model)
		{
			var generalPage = new GeneralPage(_driver);
			generalPage.ChooseWorkspace(_context.WorkspaceName);

			ExportFirstPage firstPage = SetupFirstIntegrationPointsPage(generalPage, model);

			ExportToFileSecondPage secondPage = SetupExportToFileSecondPage(firstPage, model);

			ExportToFileThirdPage thirdPage = SetupExportToFileThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}
	}
}
