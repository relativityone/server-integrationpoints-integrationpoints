using kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile;
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
				.SwitchTo().Frame("externalPage");
			WaitForPage();

			FieldMapping = new FieldMappingPanel(Driver.FindElementById("fieldMappings"));
			Settings = new SettingsPanel(Driver.FindElementByCssSelector("#pointBody > div > div:nth-child(2)"));
		}
	}
}