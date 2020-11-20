using System;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportEntityToFileSecondPage : GeneralPage
	{
		protected IWebElement NextButton=> Driver.FindElementEx(By.Id("next"));

		protected IWebElement AddAllFieldsButton=> Driver.FindElementEx(By.Id("add-all-fields"));

		protected IWebElement ViewSelectWebElement=> Driver.FindElementEx(By.Id("viewSelector"));

		protected SelectElement ViewSelect => new SelectElement(ViewSelectWebElement);

		public string View
		{
			get { return ViewSelect.SelectedOption.Text; }
			set { ViewSelect.SelectByTextEx(value, Driver); }
		}
		public ExportEntityToFileSecondPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
		}

		public ExportToFileThirdPage GoToNextPage()
		{
			NextButton.ClickEx(Driver);
			return new ExportToFileThirdPage(Driver);

		}

		public void SelectAllFields()
		{
			var wait = new WebDriverWait(Driver, TimeSpan.FromMilliseconds(500));
			wait.Until(ExpectedConditions.ElementToBeClickable(AddAllFieldsButton));
			AddAllFieldsButton.ClickEx(Driver);
		}
	}
}
