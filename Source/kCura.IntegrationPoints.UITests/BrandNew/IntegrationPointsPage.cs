using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class IntegrationPointsPage : GeneralPage
	{   
		[FindsBy(How = How.CssSelector, Using = "#dashboardPanel > div > div.dashboard-controls.new-item-button-wrapper > div > button-wgt > div > button")]
		protected IWebElement NewIntegrationPointButton { get; set; }

		public IntegrationPointsPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().DefaultContent()
				.SwitchToFrameEx(_mainFrameNameNewUi);
		}

		public void CreateNewIntegrationPoint()
		{
			WaitForPage();
			NewIntegrationPointButton.ClickEx(Driver, true);
		}
	}
}
