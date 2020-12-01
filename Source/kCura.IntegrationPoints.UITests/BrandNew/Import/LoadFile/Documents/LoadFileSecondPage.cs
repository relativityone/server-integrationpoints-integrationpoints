using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Documents
{
	public class LoadFileSecondPage : CreateIntegrationPointPage
	{
		public LoadFileSettingsPanel LoadFileSettings => new LoadFileSettingsPanel(Driver.FindElementEx(By.CssSelector("#import-provider-configuration > div:nth-child(1)")), Driver);

		public FileEncodingPanel FileEncoding { get; }

		public LoadFileSecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameOldUi)
				.SwitchToFrameEx("configurationFrame");
			WaitForPage();
			
			FileEncoding = new FileEncodingPanel(Driver.FindElementEx(By.CssSelector("#import-provider-configuration > div:nth-child(3)")), Driver);
		}
	}
}