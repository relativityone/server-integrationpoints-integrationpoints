using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
    public class ExportToFileThirdPage : GeneralPage
    {
        [FindsBy(How = How.Id, Using = "save")]
        protected IWebElement SaveButton { get; set; }

		public ExportToFileThirdPage(RemoteWebDriver driver) : base(driver)
		{
			ExportDetails = new ExportToFileThirdPageExportDetails(driver);
			LoadFileOptions = new ExportToFileThirdPageLoadFileOptions(driver);
			ImageNativeTextOptions = new ExportToFileThirdPageImageNativeTextOptions(driver);
			VolumeSubdirectoryDetails = new ExportToFileThirdPageVolumeSubdirectoryDetails(driver);

			Sleep(200);
			WaitForPage();
            PageFactory.InitElements(driver, this);
		}

	    public ExportToFileThirdPageExportDetails ExportDetails { get; set; }

	    public ExportToFileThirdPageLoadFileOptions LoadFileOptions { get; set; }

	    public ExportToFileThirdPageImageNativeTextOptions ImageNativeTextOptions { get; set; }

	    public ExportToFileThirdPageVolumeSubdirectoryDetails VolumeSubdirectoryDetails { get; set; }

		public IntegrationPointDetailsPage SaveIntegrationPoint()
        {
            SaveButton.Click();
            return new IntegrationPointDetailsPage(Driver);
        }

	}
}
