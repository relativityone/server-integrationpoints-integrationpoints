using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileSecondPage : GeneralPage
    {

        [FindsBy(How = How.Id, Using = "next")]
        protected IWebElement NextButton { get; set; }

		[FindsBy(How = How.Id, Using = "saved-search-selection-button")]
        protected IWebElement SavedSearchSelectionButton { get; set; }

		protected Select SavedSearchSelect { get; set; }

		[FindsBy(How = How.CssSelector, Using = "#s2id_savedSearchSelector a")]
        protected IWebElement SavedSearchSelectWebElement { get; set; }

		protected SelectElement SavedSearchSelectAAA => new SelectElement(SavedSearchSelectWebElement);

        public string SavedSearch
        {
            get { return SavedSearchSelectAAA.SelectedOption.Text; }
            set { SavedSearchSelectAAA.SelectByText(value); }
        }

        public ExportToFileSecondPage(RemoteWebDriver driver) : base(driver)
        {
            WaitForPage();
            PageFactory.InitElements(driver, this);
            SavedSearchSelect = new Select(Driver, "s2id_savedSearchSelector");
        }

        public ExportToFileSecondPage SelectAllDocuments()
        {
            SavedSearchSelect.Choose("All Documents");
            Sleep(200);
            return this;
        }

        public ExportToFileThirdPage GoToNextPage()
        {
			WaitForPage();
            NextButton.Click();
            return new ExportToFileThirdPage(Driver);
        }

        public SavedSearchDialog OpenSavedSearchSelectionDialog()
        {
            SavedSearchSelectionButton.Click();
            return new SavedSearchDialog(Driver);
        }

    }
}
