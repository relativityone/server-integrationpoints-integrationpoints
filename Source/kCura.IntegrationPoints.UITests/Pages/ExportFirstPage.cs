using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
    public class ExportFirstPage : GeneralPage
    {
        [FindsBy(How = How.Id, Using = "name")]
        protected IWebElement NameInput;

        public string Name
        {
            get { return NameInput.Text; }
            set { NameInput.SendKeys(value); }
        }

        [FindsBy(How = How.Id, Using = "destinationProviderType")]
        protected IWebElement DestinationSelectWebElement;

        protected SelectElement DestinationSelect => new SelectElement(DestinationSelectWebElement);

        public string Destination
        {
            get { return DestinationSelect.SelectedOption.Text; }
            set { DestinationSelect.SelectByText(value); }
        }

        [FindsBy(How = How.Id, Using = "next")]
        protected IWebElement NextButton;

        public ExportFirstPage(IWebDriver driver) : base(driver)
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
