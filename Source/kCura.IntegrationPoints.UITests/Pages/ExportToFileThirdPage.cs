using System;
using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
    public class ExportToFileThirdPage : GeneralPage
    {
		
        [FindsBy(How = How.Id, Using = "save")]
        protected IWebElement SaveButton { get; set; }

		public TreeSelect DestinationFolder { get; set; }

        public ExportToFileThirdPage(RemoteWebDriver driver) : base(driver)
        {
            WaitForPage();
            PageFactory.InitElements(driver, this);
	        DestinationFolder = new TreeSelect(driver.FindElementByXPath(@"//div[@class='field-row']/div[contains(text(), 'Destination Folder:')]/.."));
		}

        public IntegrationPointDetailsPage SaveIntegrationPoint()
        {
            SaveButton.Click();
            return new IntegrationPointDetailsPage(Driver);
        }

	}
}
