using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class ImportSecondBasePage<TModel> : GeneralPage
	{
		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }

		protected ImportSecondBasePage(RemoteWebDriver driver) : base(driver)
		{
			driver.SwitchTo().Frame("configurationFrame");
			WaitForPage();
			PageFactory.InitElements(driver, this);
		}

		public abstract void SetupModel(TModel model);
		

		protected void SetInputText(IWebElement element, string text)
		{
			element.Clear();
			element.SendKeys(text);
		}

		public ImportThirdPage<TModel> GoToNextPage(Func<ImportThirdPage<TModel>> funcThridPageCreator)
		{
			WaitForPage();
			NextButton.Click();
			return funcThridPageCreator();
		}
	}
}
