using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Actions
{
	public class IntegrationPointsImportFromLoadFileAction : IntegrationPointsImportAction
	{
		public IntegrationPointsImportFromLoadFileAction(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public IntegrationPointDetailsPage CreateNewImportFromLoadFileIntegrationPoint(ImportFromLoadFileModel model)
		{
			// TODO
			return null;
		}
	}
}
