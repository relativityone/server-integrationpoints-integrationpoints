using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile
{
	public class SettingsPanel : Component
	{
		public SimpleSelectField Overwrite => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(1)")));

		public SimpleSelectField MultiSelectFieldOverlayBehavior => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(2)")));

		public RadioField CopyNativeFiles => new RadioField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(7)")));

		public SimpleSelectField NativeFilePath => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(11)")));

		public RadioField UseFolderPathInformation => new RadioField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(13)")));

		public SimpleSelectField FolderPathInformation => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(15)")));

		public RadioField MoveExistingDocuments => new RadioField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(16)")));
		
		// Extracted text area
		public RadioField CellContainsFileLocation => new RadioField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div:nth-child(3) > div > div.field-row")));

		public SimpleSelectField CellContainingFileLocation => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div:nth-child(3) > div > div:nth-child(2) > div:nth-child(1)")));

		public SimpleSelectField EncodingForUndetectableFiles => new SimpleSelectField(Parent.FindElement(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div:nth-child(3) > div > div:nth-child(2) > div:nth-child(2)")));


		public SettingsPanel(IWebElement parent) : base(parent)
		{
		}
	}
}