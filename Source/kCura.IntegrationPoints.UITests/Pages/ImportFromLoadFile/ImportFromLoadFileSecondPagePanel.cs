using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile
{
	public abstract class ImportFromLoadFileSecondPagePanel : Page
	{
		protected ImportFromLoadFileSecondPagePanel(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
		}

		public abstract void SetupModel(ImportFromLoadFileModel model);
	}
}
