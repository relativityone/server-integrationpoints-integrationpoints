using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Productions
{
	public class SecondPage : CreateIntegrationPointPage
	{
		public LoadFileSettingsPanel LoadFileSettings => new LoadFileSettingsPanel(Driver.FindElementEx(By.CssSelector("#import-provider-configuration > div:nth-child(1)")), Driver);

		public ImportSettingsPanel ImportSettings => new ImportSettingsPanel(Driver.FindElementEx(By.CssSelector("#import-provider-configuration > div:nth-child(2)")), Driver);

		
		public SecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx("externalPage")
				.SwitchToFrameEx("configurationFrame");

			WaitForPage();
		}
	}
}