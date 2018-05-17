using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Documents
{
	public class SecondPage : CreateIntegrationPointPage
	{
		public LoadFileSettingsPanel LoadFileSettings => new LoadFileSettingsPanel(Driver.FindElementByCssSelector("#import-provider-configuration > div:nth-child(1)"));

		public FileEncodingPanel FileEncoding { get; }

		public SecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchTo().Frame("externalPage")
				.SwitchTo().Frame("configurationFrame");
			WaitForPage();
			
			FileEncoding = new FileEncodingPanel(Driver.FindElementByCssSelector("#import-provider-configuration > div:nth-child(3)"));
		}
	}
}