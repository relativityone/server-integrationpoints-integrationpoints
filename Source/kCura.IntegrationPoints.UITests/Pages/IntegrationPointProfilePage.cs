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
		protected IWebElement NewIntegrationPointProfileButton => Driver.FindElementEx(By.CssSelector("#dashboardPanel > div > div.dashboard-controls.new-item-button-wrapper > div > button-wgt > div > button"));

		public IntegrationPointProfilePage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().DefaultContent().SwitchToFrameEx(_mainFrameNameNewUi);
		}

		public ExportFirstPage CreateNewIntegrationPointProfile()
		{
			NewIntegrationPointProfileButton.ClickEx(Driver);
			return new ExportFirstPage(Driver);
		}
	}
}