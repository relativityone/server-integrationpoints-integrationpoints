using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileThirdPageVolumeSubdirectoryDetails : ExportToFileThirdPagePanel
	{
		[FindsBy(How = How.Id, Using = "volume-prefix-input")]
		protected IWebElement VolumePrefixInput { get; set; }

		[FindsBy(How = How.Id, Using = "volume-start-files-input")]
		protected IWebElement VolumeStartNumberInput { get; set; }

		[FindsBy(How = How.Id, Using = "volume-digit-padding-input")]
		protected IWebElement VolumeNumberOfDigitsInput { get; set; }

		[FindsBy(How = How.Id, Using = "volume-max-size-input")]
		protected IWebElement VolumeMaxSizeInput { get; set; }

		[FindsBy(How = How.Id, Using = "subdirectory-start-files-input")]
		protected IWebElement SubdirectoryStartNumberInput { get; set; }

		[FindsBy(How = How.Id, Using = "subdirectory-digit-padding-input")]
		protected IWebElement SubdirectoryNumberOfDigitsInput { get; set; }

		[FindsBy(How = How.Id, Using = "subdirectory-max-files-input")]
		protected IWebElement SubdirectoryMaxFilesInput { get; set; }

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
