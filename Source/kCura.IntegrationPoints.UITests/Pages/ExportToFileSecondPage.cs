using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileSecondPage : GeneralPage
    {

        [FindsBy(How = How.Id, Using = "next")]
        protected IWebElement NextButton { get; set; }

		[FindsBy(How = How.Id, Using = "saved-search-selection-button")]
        protected IWebElement SavedSearchSelectionButton { get; set; }

		public Select SavedSearch { get; }

        public ExportToFileSecondPage(RemoteWebDriver driver) : base(driver)
        {
            WaitForPage();
            PageFactory.InitElements(driver, this);
            SavedSearch = new Select(Driver.FindElementById("s2id_savedSearchSelector"));
        }

        public ExportToFileSecondPage SelectAllDocuments()
        {
            SavedSearch.Choose("All Documents");
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
            return new SavedSearchDialog(Driver.FindElementByXPath("/*"));
        }

    }
}
