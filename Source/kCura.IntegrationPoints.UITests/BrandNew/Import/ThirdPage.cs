using kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import
{
	public class ThirdPage : CreateIntegrationPointPage
	{
		public FieldMappingPanel FieldMapping { get; }

		public SettingsPanel Settings { get; }

		public ThirdPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameOldUi);
			WaitForPage();

			FieldMapping = new FieldMappingPanel(Driver.FindElementEx(By.Id("fieldMappings")), Driver);
			Settings = new SettingsPanel(Driver.FindElementEx(By.CssSelector("#pointBody > div > div:nth-child(2)")), Driver);
		}
	}
}