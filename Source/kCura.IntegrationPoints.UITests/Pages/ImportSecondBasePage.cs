using System;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class ImportSecondBasePage<TModel> : GeneralPage
	{
		protected IWebElement NextButton => Driver.FindElementEx(By.Id("next"));

		protected ImportSecondBasePage(RemoteWebDriver driver) : base(driver)
		{
			driver.SwitchToFrameEx("configurationFrame");
			WaitForPage();
			PageFactory.InitElements(driver, this);
		}

		public abstract void SetupModel(TModel model);
		
		public ImportThirdPage<TModel> GoToNextPage(Func<ImportThirdPage<TModel>> funcThridPageCreator)
		{
			Driver.SwitchTo().DefaultContent();
			Driver.SwitchToFrameEx(_mainFrameNameOldUi);
			WaitForPage();

			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().ParentFrame();
			Driver.SwitchToFrameEx(_mainFrameNameOldUi);

			NextButton.ClickEx(Driver);
			return funcThridPageCreator();
		}
	}
}
