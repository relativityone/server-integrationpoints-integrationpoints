using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.MyFirstProvider
{
	public class MyFirstProviderSecondPage : CreateIntegrationPointPage
	{
		public MyFirstProviderConfigurationPanel MyFirstProviderConfigurationPanel => new MyFirstProviderConfigurationPanel(
			Driver.FindElementEx(By.CssSelector("body:nth-child(2) > div.card:nth-child(1)")), Driver);

		public MyFirstProviderSecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameOldUi)
				.SwitchToFrameEx("configurationFrame");
			WaitForPage();
		}
	}
}