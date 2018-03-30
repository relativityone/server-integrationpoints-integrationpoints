using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Actions
{
	public class IntegrationPointsImportFTPAction : IntegrationPointsAction
	{
		public IntegrationPointsImportFTPAction(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public IntegrationPointDetailsPage CreateNewImportFromFTPIntegrationPoint(ImportFromFTPModel model)
		{
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			ImportWithFtpFirstPage firstPage = SetupImportFirstPage<ImportWithFtpFirstPage, ImportWithFTPSecondPage, ImportFromFTPModel>(generalPage, model.General,
				() => new ImportWithFtpFirstPage(Driver));

			return null;
		}
	}
}
