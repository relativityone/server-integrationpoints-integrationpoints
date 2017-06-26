using System;
using IntegrationPointsUITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace IntegrationPointsUITests.Pages
{
    public class ExportToFileSecondPage : GeneralPage
    {

        [FindsBy(How = How.Id, Using = "saved-search-selection-button")]
        protected IWebElement SavedSearchSelectionButton;

        [FindsBy(How = How.CssSelector, Using = "#s2id_savedSearchSelector .select2-chosen")]
        protected IWebElement SavedSearchSelect;

        public string SavedSearch
        {
            get { return SavedSearchSelect.Text; }
            set { throw new NotImplementedException(); }
        }

        public ExportToFileSecondPage(IWebDriver driver) : base(driver)
        {
            WaitForPage();
            PageFactory.InitElements(driver, this);
        }

        public SavedSearchDialog OpenSavedSearchSelectionDialog()
        {
            SavedSearchSelectionButton.Click();
            return new SavedSearchDialog(Driver);
        }

    }
}
