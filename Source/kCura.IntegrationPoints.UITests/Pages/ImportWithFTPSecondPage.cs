using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ImportWithFTPSecondPage : GeneralPage
	{
		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }

		public ImportWithFTPSecondPage(RemoteWebDriver driver) : base(driver)
		{
		}

		public ImportThirdPage GoToNextPage()
		{
			WaitForPage();
			NextButton.Click();
			return new ImportThirdPage(Driver);
		}
	}
}
