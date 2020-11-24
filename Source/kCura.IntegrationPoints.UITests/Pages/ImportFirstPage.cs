using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class ImportFirstPage<TSecondPage, TModel> : FirstPage where TSecondPage : ImportSecondBasePage<TModel>
	{
		protected ImportFirstPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
			driver.SwitchToFrameEx("externalPage");
			WaitForPage();
		}

		public void SelectImport()
		{
			ImportRadioButtonLabel.ClickEx(Driver);
		}

		public TSecondPage GoToNextPage() 
		{
			InitSecondPage();
			return Create(Driver);
		}

		public bool IsEntityTransferredObjectOptionAvailable() =>
			CustodianToEntityUtils.IsEntityOptionAvailable(TransferredObjectSelect);

		protected abstract TSecondPage Create(RemoteWebDriver driver);

		private void InitSecondPage()
		{
			WaitForPage();
			NextButton.ClickEx(Driver);
		}
	}
}
