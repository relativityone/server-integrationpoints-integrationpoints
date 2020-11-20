using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile
{
	public class SettingsPanel : Component
	{
		public SimpleSelectField Overwrite => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(1)")), Driver);

		public SimpleSelectField MultiSelectFieldOverlayBehavior => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(2)")), Driver);

		public RadioField CopyNativeFiles => new RadioField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(7)")), Driver);

		public SimpleSelectField NativeFilePath => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(11)")), Driver);

		public RadioField UseFolderPathInformation => new RadioField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(13)")), Driver);

		public SimpleSelectField FolderPathInformation => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(15)")), Driver);

		public RadioField MoveExistingDocuments => new RadioField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div.identifier > div:nth-child(16)")), Driver);
		
		// Extracted text area
		public RadioField CellContainsFileLocation => new RadioField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div:nth-child(3) > div > div.field-row")), Driver);

		public SimpleSelectField CellContainingFileLocation => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div:nth-child(3) > div > div:nth-child(2) > div:nth-child(1)")), Driver);

		public SimpleSelectField EncodingForUndetectableFiles => new SimpleSelectField(Parent.FindElementEx(By.CssSelector(
			"table > tbody > tr > td:nth-child(1) > div:nth-child(3) > div > div:nth-child(2) > div:nth-child(2)")), Driver);


		public SettingsPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}
	}
}