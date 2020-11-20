using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileThirdPageExportDetails : ExportToFileThirdPagePanel
	{
		protected IWebElement ImagesCheckbox => Driver.FindElementEx(By.Id("export-images-checkbox"));

		protected IWebElement NativesCheckbox => Driver.FindElementEx(By.Id("export-natives-checkbox"));

		protected IWebElement TextFieldsAsFilesCheckbox => Driver.FindElementEx(By.Id("export-text-fields-as-files-checkbox"));

		public TreeSelect DestinationFolder { get; set; }

		protected IWebElement CreateExportFolderCheckbox => Driver.FindElementEx(By.Id("create-export-directory-checkbox"));

		protected IWebElement OverwriteFilesCheckbox => Driver.FindElementEx(By.Id("overwrite-file-checkbox"));

		public ExportToFileThirdPageExportDetails(RemoteWebDriver driver) : base(driver)
		{
			DestinationFolder =
				new TreeSelect(
					driver.FindElementEx(By.XPath(@"//div[@class='field-row']/div[contains(text(), 'Destination Folder:')]/..")),
					"location-select", "jstree-holder-div", Driver);
		}

		public void SelectExportImages()
		{
			ImagesCheckbox.ClickEx(Driver);
		}

		public void SelectExportNatives()
		{
			NativesCheckbox.ClickEx(Driver);
		}

		public void SelectExportTextFieldsAsFiles()
		{
			TextFieldsAsFilesCheckbox.ClickEx(Driver);
		}

		public void DeselectDoNotCreateExportFolder()
		{
			CreateExportFolderCheckbox.ClickEx(Driver);
		}

		public void SelectOverwriteFiles()
		{
			OverwriteFilesCheckbox.ClickEx(Driver);
		}
	}
}
