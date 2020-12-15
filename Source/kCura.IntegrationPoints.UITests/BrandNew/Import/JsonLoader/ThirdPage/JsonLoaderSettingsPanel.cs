using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader.ThirdPage
{
	public class JsonLoaderSettingsPanel : Component
	{
		public SimpleSelectField Overwrite => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td > div > div:nth-child(1)")), Driver);

		public SimpleSelectField MultiSelectFieldOverlayBehavior => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td > div > div:nth-child(2)")), Driver);

		public SimpleSelectField UniqueIdentifier => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td > div > div:nth-child(9)")), Driver);

		public JsonLoaderSettingsPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}
	}
}