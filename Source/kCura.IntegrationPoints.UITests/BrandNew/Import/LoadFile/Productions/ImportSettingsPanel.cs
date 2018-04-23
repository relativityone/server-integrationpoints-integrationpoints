using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Productions
{
	public class ImportSettingsPanel : Component
	{
		public RadioField Numbering => new RadioField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(1)")));

		public SimpleSelectField ImportMode => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(2)")));

		public RadioField CopyFilesToDocumentRepository => new RadioField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(4)")));

		public SimpleSelectField FileRepository => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(5)")));

		public SimpleSelectField Production => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(7)")));


		public ImportSettingsPanel(IWebElement parent) : base(parent)
		{
		}

	}
}
