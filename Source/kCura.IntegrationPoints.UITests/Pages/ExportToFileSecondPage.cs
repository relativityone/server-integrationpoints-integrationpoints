using IntegrationPointsUITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace IntegrationPointsUITests.Pages
{

    public class Select
    {
        protected IWebElement SelectLink => _driver.FindElement(By.CssSelector($"#{_id} a"));
        protected IWebElement Dropdown => _driver.FindElement(By.Id("select2-drop"));
        protected IWebElement DropdownSearch => Dropdown.FindElement(By.TagName("input"));
        
        private readonly string _id;
        private readonly IWebDriver _driver;

        public Select(IWebDriver driver, string id)
        {
            _driver = driver;
            _id = id;
        }

        protected Select Toggle()
        {
            SelectLink.Click();
            return this;
        }

        protected bool IsOpen()
        {
            return _driver.FindElement(By.Id(_id)).GetCssValue("class").Contains("select2-dropdown-open");
        }

        protected Select EnsureOpen()
        {
            if (!IsOpen())
            {
                Toggle();
            }
            return this;
        }

        // read current value
        // read all values

        public Select Choose(string element)
        {
            EnsureOpen();
            DropdownSearch.SendKeys(element + Keys.Enter);
            return this;
        }
    }

    public class ExportToFileSecondPage : GeneralPage
    {

        [FindsBy(How = How.Id, Using = "saved-search-selection-button")]
        protected IWebElement SavedSearchSelectionButton;

        protected Select SavedSearchSelect;
        
        [FindsBy(How = How.CssSelector, Using = "#s2id_savedSearchSelector a")]
        protected IWebElement SavedSearchSelectWebElement;

        protected SelectElement SavedSearchSelectAAA => new SelectElement(SavedSearchSelectWebElement);

        public string SavedSearch
        {
            get { return SavedSearchSelectAAA.SelectedOption.Text; }
            set { SavedSearchSelectAAA.SelectByText(value); }
        }

        [FindsBy(How = How.Id, Using = "next")]
        protected IWebElement NextButton;

        public ExportToFileSecondPage(IWebDriver driver) : base(driver)
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
