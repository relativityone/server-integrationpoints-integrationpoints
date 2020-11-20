using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileThirdPageVolumeSubdirectoryDetails : ExportToFileThirdPagePanel
	{
		protected IWebElement VolumePrefixInput => Driver.FindElementEx(By.Id("volume-prefix-input"));

		protected IWebElement VolumeStartNumberInput => Driver.FindElementEx(By.Id("volume-start-files-input"));

		protected IWebElement VolumeNumberOfDigitsInput => Driver.FindElementEx(By.Id("volume-digit-padding-input"));

		protected IWebElement VolumeMaxSizeInput => Driver.FindElementEx(By.Id("volume-max-size-input"));

		protected IWebElement SubdirectoryStartNumberInput => Driver.FindElementEx(By.Id("subdirectory-start-files-input"));

		protected IWebElement SubdirectoryNumberOfDigitsInput => Driver.FindElementEx(By.Id("subdirectory-digit-padding-input"));

		protected IWebElement SubdirectoryMaxFilesInput => Driver.FindElementEx(By.Id("subdirectory-max-files-input"));

		public ExportToFileThirdPageVolumeSubdirectoryDetails(RemoteWebDriver driver) : base(driver)
		{
		}

		public string VolumePrefix
		{
			get { return VolumePrefixInput.Text; }
			set { SetInputText(VolumePrefixInput, value); }
		}

		public string VolumeStartNumber
		{
			get { return VolumeStartNumberInput.Text; }
			set { SetInputText(VolumeStartNumberInput, value); }
		}

		public string VolumeNumberOfDigits
		{
			get { return VolumeNumberOfDigitsInput.Text; }
			set { SetInputText(VolumeNumberOfDigitsInput, value); }
		}

		public string VolumeMaxSize
		{
			get { return VolumeMaxSizeInput.Text; }
			set { SetInputText(VolumeMaxSizeInput, value); }
		}

		public string SubdirectoryStartNumber
		{
			get { return SubdirectoryStartNumberInput.Text; }
			set { SetInputText(SubdirectoryStartNumberInput, value); }
		}

		public string SubdirectoryNumberOfDigits
		{
			get { return SubdirectoryNumberOfDigitsInput.Text; }
			set { SetInputText(SubdirectoryNumberOfDigitsInput, value); }
		}

		public string SubdirectoryMaxFiles
		{
			get { return SubdirectoryMaxFilesInput.Text; }
			set { SetInputText(SubdirectoryMaxFilesInput, value); }
		}
	}
}
