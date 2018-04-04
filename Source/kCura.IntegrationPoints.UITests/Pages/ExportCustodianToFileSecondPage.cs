using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
    public class ExportCustodianToFileSecondPage : GeneralPage
    {
        [FindsBy(How = How.Id, Using = "next")]
        protected IWebElement NextButton { get; set; }

        [FindsBy(How = How.Id, Using = "add-all-fields")]
        protected IWebElement AddAllFieldsButton { get; set; }

        [FindsBy(How = How.Id, Using = "viewSelector")]
        protected IWebElement ViewSelectWebElement { get; set; }
        
        protected SelectElement ViewSelect => new SelectElement(ViewSelectWebElement);

        public string View
        {
            get { return ViewSelect.SelectedOption.Text; }
            set { ViewSelect.SelectByText(value); }
        }
        public ExportCustodianToFileSecondPage(RemoteWebDriver driver) : base(driver)
        {
            PageFactory.InitElements(driver, this);
        }

        public ExportToFileThirdPage GoToNextPage()
        {
            NextButton.Click();
            return new ExportToFileThirdPage(Driver);

        }

        public void SelectAllFields()
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromMilliseconds(500));
            wait.Until(ExpectedConditions.ElementToBeClickable(AddAllFieldsButton));
            AddAllFieldsButton.Click();
        }
    }
}
