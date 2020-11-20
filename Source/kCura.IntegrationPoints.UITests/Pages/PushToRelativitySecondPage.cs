using System;
using System.Collections.Generic;
using System.Threading;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class PushToRelativitySecondPage : GeneralPage
	{
		protected IWebElement SourceSelectWebElement => Driver.FindElementEx(By.Id("sourceSelector"));

		protected IWebElement DestinationSelectWebElement => Driver.FindElementEx(By.Id("workspaceSelector"));

		protected IWebElement InstanceSelectWebElement => Driver.FindElementEx(By.Id("relativitySelector"));

		protected IWebElement NextButton => Driver.FindElementEx(By.Id("next"));

		protected IWebElement FolderLocation => Driver.FindElementEx(By.Id("location-0"));

		protected IWebElement ProductionLocation => Driver.FindElementEx(By.Id("location-1"));

		protected IWebElement SourceProductionSelectWebElement => Driver.FindElementEx(By.Id("s2id_sourceProductionSetsSelector"));

		protected IWebElement ProductionLocationSelectWebElement => Driver.FindElementEx(By.Id("s2id_productionSetsSelector"));

		protected IWebElement FolderLocationSelectTextWebElement => Driver.FindElementEx(By.Id("location-input"));

		public IList<IWebElement> listOfValidationErrorsElements => Driver.FindElementsEx(By.ClassName("field-validation-error"));

		protected Select SourceProductionSelect => new Select(SourceProductionSelectWebElement, Driver);

		protected Select ProductionLocationSelect => new Select(ProductionLocationSelectWebElement, Driver);


		protected SelectElement SourceSelectElement => new SelectElement(SourceSelectWebElement);
		protected SavedSearchSelector SavedSearchSelector { get; }

		public string SourceSelect
		{
			get { return SourceSelectElement.SelectedOption.Text; }
			set { SourceSelectElement.SelectByTextEx(value, Driver); }
		}

		protected SelectElement DestinationWorkspaceElement => new SelectElement(DestinationSelectWebElement);

		public string DestinationWorkspace
		{
			get { return DestinationWorkspaceElement.SelectedOption.Text; }
			set { DestinationWorkspaceElement.SelectByTextEx(value, Driver); }
		}

		protected SelectElement RelativityInstanceElement => new SelectElement(InstanceSelectWebElement);

		public TreeSelect FolderLocationSelect;

		public string FolderLocationSelectText => FolderLocationSelectTextWebElement.Text;

		public string RelativityInstance
		{
			get { return RelativityInstanceElement.SelectedOption.Text; }
			set { RelativityInstanceElement.SelectByTextEx(value, Driver); }
		}

		public PushToRelativitySecondPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			driver.SwitchToFrameEx("configurationFrame");

			Thread.Sleep(1500);


			FolderLocationSelect = new TreeSelect(driver.FindElementEx(By.XPath(@"//div[@id='location-select']/..")),
				"location-select", "jstree-holder-div", Driver);
			SavedSearchSelector = new SavedSearchSelector(Driver);
		}

		public PushToRelativitySecondPage SelectSavedSearch(string savedSearchName)
		{
			SavedSearchSelector.SelectSavedSearch(savedSearchName);
			return this;
		}

		public string GetSelectedSavedSearch()
		{
			return SavedSearchSelector.GetSelectedSavedSearch();
		}

		public PushToRelativitySecondPage SelectSourceProduction(string productionName)
		{
			WaitForPage();
			SourceProductionSelect.Choose(productionName);
			WaitForPage();
			return this;
		}

		public PushToRelativitySecondPage SelectFolderLocation()
		{
			WaitForPage();
			const int timeoutInSeconds = 40;
			FolderLocation.ClickEx(Driver, timeout: TimeSpan.FromSeconds(timeoutInSeconds));
			return this;
		}

		public PushToRelativitySecondPage SelectProductionLocation(string productionName)
		{
			ProductionLocation.ClickEx(Driver);
			WaitForPage();
			ProductionLocationSelect.Choose(productionName);
			return this;
		}

		public PushToRelativityThirdPage GoToNextPage()
		{
			WaitForPage();

			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().ParentFrame();
			Driver.SwitchToFrameEx(_mainFrameNameOldUi);

			NextButton.ClickEx(Driver);
			return new PushToRelativityThirdPage(Driver);
		}
	}
}