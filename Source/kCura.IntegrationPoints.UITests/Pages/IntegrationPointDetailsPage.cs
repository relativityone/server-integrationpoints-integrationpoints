using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
    public class IntegrationPointDetailsPage : GeneralPage
    {
        [FindsBy(How = How.LinkText, Using = "Edit")]
        protected IWebElement EditButton;

        [FindsBy(How = How.LinkText, Using = "Delete")]
        protected IWebElement DeleteButton;

        [FindsBy(How = How.LinkText, Using = "Back")]
        protected IWebElement BackButton;

        [FindsBy(How = How.LinkText, Using = "Edit Permissions")]
        protected IWebElement EditPermissionsButton;

        [FindsBy(How = How.LinkText, Using = "View Audit")]
        protected IWebElement ViewAuditButton;

        [FindsBy(How = How.LinkText, Using = "Run")]
        protected IWebElement RunButton;

        [FindsBy(How = How.LinkText, Using = "Save as a Profile")]
        protected IWebElement SaveProfileButton;

        [FindsBy(How = How.LinkText, Using = "General")]
        protected IWebElement GeneralTabLink;
        
        public IntegrationPointDetailsPage(IWebDriver driver) : base(driver)
        {
            WaitForPage();
            PageFactory.InitElements(driver, this);
            //Driver.SwitchTo().Frame("externalPage");
        }

        public IntegrationPointDetailsPage RunIntegrationPoint()
        {
            RunButton.Click();
            return this;
        }

        public PropertiesTable SelectGeneralPropertiesTable()
        {
            var t = new PropertiesTable(Driver, "General", "summaryPage");
            t.Select();
            return t;
        }

        public PropertiesTable SelectSchedulingPropertiesTable()
        {
            var t = new PropertiesTable(Driver, "Scheduling", "schedulerSummaryPage");
            t.Select();
            return t;
        }
        
    }
}
