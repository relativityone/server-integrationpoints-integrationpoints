using System;
using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
    public class ExportToFileThirdPage : GeneralPage
    {
        
        [FindsBy(How = How.Id, Using = "save")]
        protected IWebElement SaveButton;

        public ExportToFileThirdPage(IWebDriver driver) : base(driver)
        {
            WaitForPage();
            PageFactory.InitElements(driver, this);
        }

        public IntegrationPointDetailsPage SaveIntegrationPoint()
        {
            SaveButton.Click();
            return new IntegrationPointDetailsPage(Driver);
        }
        
    }
}
