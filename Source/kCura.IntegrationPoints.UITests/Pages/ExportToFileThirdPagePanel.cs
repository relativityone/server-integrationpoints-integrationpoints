using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;

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
