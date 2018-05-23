using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Images
{
	public class SecondPage : CreateIntegrationPointPage
	{
		public LoadFileSettingsPanel LoadFileSettings => new LoadFileSettingsPanel(Driver.FindElementByCssSelector("#import-provider-configuration > div:nth-child(1)"));

		public ImportSettingsPanel ImportSettings => new ImportSettingsPanel(Driver.FindElementByCssSelector("#import-provider-configuration > div:nth-child(2)"));

		
		public SecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchTo().Frame("externalPage")
				.SwitchTo().Frame("configurationFrame");

			WaitForPage();
		}
	}
}