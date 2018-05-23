using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Images
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

		public RadioField LoadExtractedText => new RadioField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(6) > div.field-row")));

		public SimpleSelectField EncodingForUndetectableFiles => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td > div:nth-child(6) > div:nth-child(2) > div")));

		public ImportSettingsPanel(IWebElement parent) : base(parent)
		{
		}

	}
}
