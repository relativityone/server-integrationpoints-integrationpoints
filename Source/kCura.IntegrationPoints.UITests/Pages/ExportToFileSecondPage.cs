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

		[FindsBy(How = How.Id, Using = "available-fields")]
		protected IWebElement SourceFieldsElement { get; set; }

		[FindsBy(How = How.Id, Using = "start-export-at-record")]
		protected IWebElement StartExportAtRecordElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-field")]
		protected IWebElement AddSourceFieldElement { get; set; }

		[FindsBy(How = How.Id, Using = "add-all-fields")]
		protected IWebElement AddAllSourceFieldElements { get; set; }

		[FindsBy(How = How.Id, Using = "sourceSelector")]
		protected IWebElement SourceSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "productionSetsSelector")]
		protected IWebElement ProductionSetSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "location-select")]
		protected IWebElement FolderLocationTreeWebElement { get; set; }

		public Select SavedSearch { get; }
		protected SelectElement SelectSourceFieldsElement => new SelectElement(SourceFieldsElement);
		protected SelectElement SourceSelect => new SelectElement(SourceSelectWebElement);
		protected SelectElement ProductionSetSelect => new SelectElement(ProductionSetSelectWebElement);
		protected TreeSelect FolderLocationTree => new TreeSelect(FolderLocationTreeWebElement);

		public ExportToFileSecondPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			SavedSearch = new Select(Driver.FindElementById("s2id_savedSearchSelector"));
		}

		public string Source
		{
			get { return SourceSelect.SelectedOption.Text; }
			set { SourceSelect.SelectByText(value); }
		}

		public string ProductionSet
		{
			get { return ProductionSetSelect.SelectedOption.Text; }
			set { ProductionSetSelect.SelectByText(value); }
		}

		public string Folder
		{
			set { FolderLocationTree.ChooseChildElement(value); }
		}

		public ExportToFileSecondPage SelectSavedSearch(string savedSearch)
		{
			SavedSearch.Choose(savedSearch);
			Sleep(200);
			return this;
		}

		public int StartExportAtRecord
		{
			get { return int.Parse(StartExportAtRecordElement.Text); }
			set { SetInputText(StartExportAtRecordElement, value.ToString()); }
		}

		public ExportToFileSecondPage SelectAllDocumentsSavedSearch()
		{
			return SelectSavedSearch("All Documents");
		}

		public void SelectAllSourceFields()
		{
			AddAllSourceFieldElements.Click();
		}

		public void SelectSourceField(string fieldName)
		{
			SelectField(SelectSourceFieldsElement, AddSourceFieldElement, fieldName);
		}

		protected void SelectField(SelectElement selectElement, IWebElement addFieldElement, string fieldName)
		{
			if (string.IsNullOrEmpty(fieldName))
			{
				return;
			}

			SelectOption(selectElement, fieldName);

			addFieldElement.Click();
		}

		private static void SelectOption(SelectElement selectElement, string textToSearchFor)
		{
			IWebElement option = selectElement.WrappedElement.FindElement(By.XPath($".//option[starts-with(normalize-space(.), \"{textToSearchFor}\")]"));
			if (!option.Selected)
			{
				option.Click();
			}
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
