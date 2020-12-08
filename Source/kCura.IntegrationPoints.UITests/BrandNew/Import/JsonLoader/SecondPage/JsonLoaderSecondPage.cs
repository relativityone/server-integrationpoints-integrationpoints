using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader.SecondPage
{
	public class JsonLoaderSecondPage : CreateIntegrationPointPage
	{
		public JsonLoaderConfigurationPanel JsonLoaderConfigurationPanel => new JsonLoaderConfigurationPanel(
			Driver.FindElementEx(By.Id("jsonConfiguration")),
			Driver);

		public JsonLoaderSecondPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameOldUi)
				.SwitchToFrameEx("configurationFrame");
			WaitForPage();
		}
	}
}