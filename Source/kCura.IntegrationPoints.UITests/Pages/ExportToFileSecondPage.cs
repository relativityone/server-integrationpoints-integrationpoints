using System;
using System.Threading;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;
using Polly;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ExportToFileSecondPage : GeneralPage
	{
		protected IWebElement NextButton => Driver.FindElementEx(By.Id("next"));

		protected IWebElement SavedSearchSelectionButton => Driver.FindElementEx(By.Id("saved-search-selection-button"));

		protected IWebElement SourceFieldsElement => Driver.FindElementEx(By.Id("available-fields"));

		protected IWebElement StartExportAtRecordElement => Driver.FindElementEx(By.Id("start-export-at-record"));

		protected IWebElement AddSourceFieldElement => Driver.FindElementEx(By.Id("add-field"));

		protected IWebElement AddAllSourceFieldElements => Driver.FindElementEx(By.Id("add-all-fields"));

		protected IWebElement SourceSelectWebElement => Driver.FindElementEx(By.Id("sourceSelector"));

		protected IWebElement ProductionSetSelectWebElement => Driver.FindElementEx(By.Id("productionSetsSelector"));

		protected IWebElement FolderLocationTreeWebElement => Driver.FindElementEx(By.XPath("//*[@id='location-select']/.."));

		protected IWebElement ViewSelectWebElement => Driver.FindElementEx(By.Id("viewSelector"));
		protected SelectElement SelectSourceFieldsElement => new SelectElement(SourceFieldsElement);
		protected SelectElement SourceSelect => new SelectElement(SourceSelectWebElement);
		protected SelectElement ProductionSetSelect => new SelectElement(ProductionSetSelectWebElement);
		protected TreeSelect FolderLocationTree => new TreeSelect(FolderLocationTreeWebElement, "location-select", "jstree-holder-div", Driver);
		protected SelectElement ViewSelect => new SelectElement(ViewSelectWebElement);

		protected SavedSearchSelector SavedSearchSelector { get; }

		public ExportToFileSecondPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			SavedSearchSelector = new SavedSearchSelector(Driver);
		}

		public string Source
		{
			get { return SourceSelect.SelectedOption.Text; }
			set { SourceSelect.SelectByTextEx(value, Driver); }
		}

		public string ProductionSet
		{
			get { return ProductionSetSelect.SelectedOption.Text; }
			set { ProductionSetSelect.SelectByTextEx(value, Driver); }
		}

		public string Folder
		{
			set { FolderLocationTree.ChooseChildElement(value); }
		}

		public string View
		{
			get { return ViewSelect.SelectedOption.Text; }
			set { ViewSelect.SelectByTextEx(value, Driver); }
		}

		public ExportToFileSecondPage SelectSavedSearch(string savedSearch)
		{
			SavedSearchSelector.SelectSavedSearch(savedSearch);
			return this;
		}

		public string GetSelectedSavedSearch()
		{
			return SavedSearchSelector.GetSelectedSavedSearch();
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
			IWebElement selectAllButton = AddAllSourceFieldElements;
			SelectElement mappedSourceFields = SelectSourceFieldsElement;
			Driver.GetConfiguredWait().Until(d =>
				{
					selectAllButton.Click();
					return mappedSourceFields.Options.Count != 0;
				}
				);
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

			addFieldElement.ClickEx(Driver);
		}

		private void SelectOption(SelectElement selectElement, string textToSearchFor)
		{
			IWebElement option = selectElement.WrappedElement.FindElementEx(By.XPath($".//option[starts-with(normalize-space(.), \"{textToSearchFor}\")]"));
			if (!option.Selected)
			{
				option.ClickEx(Driver);
			}
		}

		public ExportToFileThirdPage GoToNextPage()
		{
			WaitForPage();
			Thread.Sleep(1000);
			NextButton.ClickEx(Driver);
			return new ExportToFileThirdPage(Driver);
		}

		public SavedSearchDialog OpenSavedSearchSelectionDialog()
		{
			SavedSearchSelectionButton.ClickEx(Driver);
			return new SavedSearchDialog(Driver.FindElementEx(By.XPath("/*")), Driver);
		}

	}
}
