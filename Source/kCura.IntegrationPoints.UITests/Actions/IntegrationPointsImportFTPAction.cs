using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Pages.FTP;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Actions
{
	public class IntegrationPointsImportFTPAction : IntegrationPointsImportAction
	{
		public IntegrationPointsImportFTPAction(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public IntegrationPointDetailsPage CreateNewImportFromFTPIntegrationPoint(ImportFromFTPModel model)
		{
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			ImportWithFTPFirstPage firstPage =
				SetupImportFirstPage<ImportWithFTPFirstPage, ImportWithFTPSecondPage, ImportFromFTPModel>(generalPage, model.General,
				() => new ImportWithFTPFirstPage(Driver));

			ImportWithFTPSecondPage secondPage =
				SetupImportSecondPage(firstPage, model);

			ImportThirdPage<ImportFromFTPModel> thirdPage =
				SetupImportThirdPage(secondPage, model, () => new ImportWithFTPThirdPage(Driver));

			return thirdPage.SaveIntegrationPoint();
		}
	}
}
