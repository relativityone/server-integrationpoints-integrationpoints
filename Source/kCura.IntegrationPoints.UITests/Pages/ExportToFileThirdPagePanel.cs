using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class ExportToFileThirdPagePanel : Page
	{
		protected ExportToFileThirdPagePanel(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
		}
	}
}
