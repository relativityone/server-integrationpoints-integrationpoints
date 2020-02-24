using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class IntegrationPointProfilePage : GeneralPage
	{
        [FindsBy(How = How.CssSelector, Using = "#dashboardPanel > div > div.dashboard-controls.new-item-button-wrapper > div > button-wgt > div > button")]
		protected IWebElement NewIntegrationPointProfileButton;

		public IntegrationPointProfilePage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().DefaultContent().SwitchTo().Frame(_mainFrameNameNewUi);
		}

		public ExportFirstPage CreateNewIntegrationPointProfile()
		{
			NewIntegrationPointProfileButton.ClickEx();
			return new ExportFirstPage(Driver);
		}
	}
}