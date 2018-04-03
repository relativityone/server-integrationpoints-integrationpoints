using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileThirdPageExportDetails : ExportToFileThirdPagePanel
	{
		[FindsBy(How = How.Id, Using = "export-images-checkbox")]
		protected IWebElement ImagesCheckbox { get; set; }

		[FindsBy(How = How.Id, Using = "export-natives-checkbox")]
		protected IWebElement NativesCheckbox { get; set; }

		[FindsBy(How = How.Id, Using = "export-text-fields-as-files-checkbox")]
		protected IWebElement TextFieldsAsFilesCheckbox { get; set; }

		public TreeSelect DestinationFolder { get; set; }

		[FindsBy(How = How.Id, Using = "create-export-directory-checkbox")]
		protected IWebElement CreateExportFolderCheckbox { get; set; }

		[FindsBy(How = How.Id, Using = "overwrite-file-checkbox")]
		protected IWebElement OverwriteFilesCheckbox { get; set; }

		public ExportToFileThirdPageExportDetails(RemoteWebDriver driver) : base(driver)
		{
			DestinationFolder = new TreeSelect(driver.FindElementByXPath(@"//div[@class='field-row']/div[contains(text(), 'Destination Folder:')]/.."));
		}

		public void SelectExportImages()
		{
			ImagesCheckbox.ClickWhenClickable();
		}

		public void SelectExportNatives()
		{
			NativesCheckbox.Click();
		}

		public void SelectExportTextFieldsAsFiles()
		{
			TextFieldsAsFilesCheckbox.Click();
		}

		public void DeselectDoNotCreateExportFolder()
		{
			CreateExportFolderCheckbox.Click();
		}

		public void SelectOverwriteFiles()
		{
			OverwriteFilesCheckbox.Click();
		}
	}
}
