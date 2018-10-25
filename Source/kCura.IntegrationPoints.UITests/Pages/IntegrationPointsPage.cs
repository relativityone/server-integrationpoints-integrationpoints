using System;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class IntegrationPointsPage : GeneralPage
	{
		private IWebElement NewIntegrationPointButton => Driver.FindElementEx(By.CssSelector("#dashboardPanel > div > div.dashboard-controls.new-item-button-wrapper > div > button-wgt > div > button"));

		public IntegrationPointsPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().DefaultContent().SwitchTo().Frame("externalPage");
		}

		public ExportFirstPage CreateNewExportIntegrationPoint()
		{
			NewIntegrationPointButton.ClickEx();
			return new ExportFirstPage(Driver);
		}

		public TImportFirstPage CreateNewImportIntegrationPoint<TImportFirstPage, TSecondPage, TModel>(Func<TImportFirstPage> funcFirstPageCreator)
			where TImportFirstPage : ImportFirstPage<TSecondPage, TModel>
			where TSecondPage : ImportSecondBasePage<TModel>
			
		{
			NewIntegrationPointButton.ClickEx();
			return funcFirstPageCreator();
		}
	}
}
