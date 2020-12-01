using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.O365
{
	public class O365SecondPage : CreateIntegrationPointPage
	{
		public O365SettingsPanel O365SettingsPanel => new O365SettingsPanel(
			Driver.FindElementEx(By.Id("tree-card")),
			Driver);

		public O365SecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameOldUi)
				.SwitchToFrameEx("configurationFrame");
			WaitForPage();
		}
	}
}