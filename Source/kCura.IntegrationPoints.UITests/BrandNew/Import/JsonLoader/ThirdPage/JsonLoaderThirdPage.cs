using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader.ThirdPage
{
	public class JsonLoaderThirdPage : CreateIntegrationPointPage
	{
		public FieldMappingPanel FieldMapping { get; }

		public JsonLoaderSettingsPanel Settings { get; }

		public JsonLoaderThirdPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameOldUi);
			WaitForPage();

			FieldMapping = new FieldMappingPanel(Driver.FindElementEx(By.Id("fieldMappings")), Driver);
			Settings = new JsonLoaderSettingsPanel(Driver.FindElementEx(By.CssSelector("#pointBody > div > div:nth-child(2)")), Driver);
		}
	}
}