using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class IntegrationPointsPage : GeneralPage
	{

		[FindsBy(How = How.XPath, Using = "//button[.='New Integration Point']")]
		protected IWebElement NewIntegrationPointButton;

		public IntegrationPointsPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().Frame("externalPage");
		}

		public ExportFirstPage CreateNewExportIntegrationPoint()
		{
			NewIntegrationPointButton.Click();
			return new ExportFirstPage(Driver);
		}

		public TImportFirstPage CreateNewImportIntegrationPoint<TImportFirstPage, TSecondPage, TModel>(Func<TImportFirstPage> funcFirstPageCreator)
			where TImportFirstPage : ImportFirstPage<TSecondPage, TModel>
			where TSecondPage : ImportSecondBasePage<TModel>
			
		{
			NewIntegrationPointButton.Click();
			return funcFirstPageCreator();
		}
	}
}
