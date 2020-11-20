using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Productions
{
	public class ImportSettingsPanel : Component
	{
		public RadioField Numbering => new RadioField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(1)")), Driver);

		public SimpleSelectField ImportMode => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(2)")), Driver);

		public RadioField CopyFilesToDocumentRepository => new RadioField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(4)")), Driver);

		public SimpleSelectField FileRepository => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(5)")), Driver);

		public SimpleSelectField Production => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(7)")), Driver);


		public ImportSettingsPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}

	}
}
