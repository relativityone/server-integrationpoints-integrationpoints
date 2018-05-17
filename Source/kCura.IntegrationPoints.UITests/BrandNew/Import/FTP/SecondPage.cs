using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.FTP
{
	public class SecondPage : CreateIntegrationPointPage
	{
		public ConnectionAndFileInfoPanel InfoPanel =>
			new ConnectionAndFileInfoPanel(Driver.FindElementByCssSelector("body > div:nth-child(1) > div"));

		public SecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchTo().Frame("externalPage")
				.SwitchTo().Frame("configurationFrame");
			WaitForPage();
		}
	}
}