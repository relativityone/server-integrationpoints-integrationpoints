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

		protected SelectElement DestinationSelect => new SelectElement(DestinationSelectWebElement);

		public string Destination
		{
			get { return DestinationSelect.SelectedOption.Text; }
			set { DestinationSelect.SelectByText(value); }
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
	}
}
