using System;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

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
		
		public ImportThirdPage<TModel> GoToNextPage(Func<ImportThirdPage<TModel>> funcThridPageCreator)
		{
			Driver.SwitchTo().DefaultContent();
			Driver.SwitchTo().Frame("_externalPage");
			WaitForPage();

			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().Frame("externalPage");

			NextButton.ClickWhenClickable();
			return funcThridPageCreator();
		}
	}
}
