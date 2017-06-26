﻿using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace IntegrationPointsUITests.Pages
{
    public class IntegrationPointsPage : GeneralPage
    {

        [FindsBy(How = How.XPath, Using = "//button[.='New Integration Point']")]
        protected IWebElement NewIntegrationPointButton;

        public IntegrationPointsPage(IWebDriver driver) : base(driver)
        {
            WaitForPage();
            PageFactory.InitElements(driver, this);
            Driver.SwitchTo().Frame("externalPage");
        }

        public ExportFirstPage CreateNewIntegrationPoint()
        {
            NewIntegrationPointButton.Click();
            return new ExportFirstPage(Driver);
        }

    }
}
