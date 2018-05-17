using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class IntegrationPointsPage : GeneralPage
	{

		[FindsBy(How = How.XPath, Using = "//button[.='New Integration Point']")]
		protected IWebElement NewIntegrationPointButton { get; set; }

		public IntegrationPointsPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().DefaultContent()
				.SwitchTo().Frame("externalPage");
		}

		public void CreateNewIntegrationPoint()
		{
			NewIntegrationPointButton.ClickWhenClickable();
		}
	}
}
