using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class FirstPage : CreateIntegrationPointPage
	{
		public GeneralPanel General { get; }

		public FirstPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameOldUi);
			WaitForPage();

			General = new GeneralPanel(Driver.FindElementEx(By.CssSelector("#pointBody > div > div:nth-child(1)")), Driver);
		}
	}
}