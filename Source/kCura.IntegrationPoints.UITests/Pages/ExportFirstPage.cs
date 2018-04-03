using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportFirstPage : FirstPage
	{
		[FindsBy(How = How.Id, Using = "destinationProviderType")]
		protected IWebElement DestinationSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "destinationRdo")]
		protected IWebElement TransferedObjectSelectWebElement { get; set; }
        
		protected SelectElement DestinationSelect => new SelectElement(DestinationSelectWebElement);
		protected SelectElement TransferedObjectSelect => new SelectElement(TransferedObjectSelectWebElement);

		public string Destination
		{
			get { return DestinationSelect.SelectedOption.Text; }
			set { DestinationSelect.SelectByText(value); }
		}

		public string TransferedObject
		{
			get { return TransferedObjectSelect.SelectedOption.Text; }
			set { TransferedObjectSelect.SelectByText(value); }
		}

		public ExportFirstPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().Frame("externalPage");
			WaitForPage();
		}

		public ExportToFileSecondPage GoToNextPage()
		{
			NextButton.Click();
			return new ExportToFileSecondPage(Driver);
		}

		public PushToRelativitySecondPage GoToNextPagePush()
		{
			NextButton.Click();
			return new PushToRelativitySecondPage(Driver);
		}

		public ExportCustodianToFileSecondPage GotoNextPageCustodian()
		{
			NextButton.Click();
			return new ExportCustodianToFileSecondPage(Driver);
		}
	}
}
