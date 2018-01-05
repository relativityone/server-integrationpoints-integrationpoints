using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
    public class ExportFirstPage : GeneralPage
    {

        [FindsBy(How = How.Id, Using = "destinationProviderType")]
        protected IWebElement DestinationSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "next")]
        protected IWebElement NextButton { get; set; }

		[FindsBy(How = How.Id, Using = "name")]
        protected IWebElement NameInput { get; set; }

		public string Name
        {
            get { return NameInput.Text; }
            set { NameInput.SendKeys(value); }
        }

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
        
    }
}
