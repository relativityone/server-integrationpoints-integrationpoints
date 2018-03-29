using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class ImportSecondBasePage<TModel> : GeneralPage
	{
		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }

		public ImportSecondBasePage(RemoteWebDriver driver) : base(driver)
		{
		}

		public abstract void SetupModel(TModel model);
		

		protected void SetInputText(IWebElement element, string text)
		{
			element.Clear();
			element.SendKeys(text);
		}

		public ImportThirdPage GoToNextPage()
		{
			WaitForPage();
			NextButton.Click();
			return new ImportThirdPage(Driver);
		}
	}
}
