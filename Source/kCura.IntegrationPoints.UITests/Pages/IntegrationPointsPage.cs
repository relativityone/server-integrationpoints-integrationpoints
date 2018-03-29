using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;

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

		public ImportFirstPage CreateNewImportIntegrationPoint()
		{
			NewIntegrationPointButton.Click();
			return new ImportFirstPage(Driver);
		}
	}
}
