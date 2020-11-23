using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.FTP
{
	public class SecondPage : CreateIntegrationPointPage
	{
		public ConnectionAndFileInfoPanel InfoPanel =>
			new ConnectionAndFileInfoPanel(Driver.FindElementEx(By.CssSelector("body > div:nth-child(1) > div")), Driver);

		public SecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameOldUi)
				.SwitchToFrameEx("configurationFrame");
			WaitForPage();
		}
	}
}