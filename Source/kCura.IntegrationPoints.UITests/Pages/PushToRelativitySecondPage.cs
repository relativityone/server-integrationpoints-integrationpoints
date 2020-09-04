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
		[FindsBy(How = How.Id, Using = "sourceSelector")]
		protected IWebElement SourceSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "workspaceSelector")]
		protected IWebElement DestinationSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "relativitySelector")]
		protected IWebElement InstanceSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }

		[FindsBy(How = How.Id, Using = "location-0")]
		protected IWebElement FolderLocation;

		[FindsBy(How = How.Id, Using = "location-1")]
		protected IWebElement ProductionLocation;

		[FindsBy(How = How.Id, Using = "s2id_sourceProductionSetsSelector")]
		protected IWebElement SourceProductionSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "s2id_productionSetsSelector")]
		protected IWebElement ProductionLocationSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "location-input")]
		protected IWebElement FolderLocationSelectTextWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "validationErrorId")]
		public IList<IWebElement> listOfValidationErrorsElements { get; set; }

		protected Select SourceProductionSelect => new Select(SourceProductionSelectWebElement);

		protected Select ProductionLocationSelect => new Select(ProductionLocationSelectWebElement);

		protected Select DestinationWorkspaceSelect { get; set; }

		protected Select RelativityInstanceSelect { get; set; }

		protected SelectElement SourceSelectElement => new SelectElement(SourceSelectWebElement);
		protected SavedSearchSelector SavedSearchSelector { get; }

		public string SourceSelect
		{
			get { return SourceSelectElement.SelectedOption.Text; }
			set { SourceSelectElement.SelectByText(value); }
		}

		protected SelectElement DestinationWorkspaceElement => new SelectElement(DestinationSelectWebElement);

		public string DestinationWorkspace
		{
			get { return DestinationWorkspaceElement.SelectedOption.Text; }
			set { DestinationWorkspaceElement.SelectByText(value); }
		}

		protected SelectElement RelativityInstanceElement => new SelectElement(InstanceSelectWebElement);

		public TreeSelect FolderLocationSelect;

		public string FolderLocationSelectText => FolderLocationSelectTextWebElement.Text;

		public string RelativityInstance
		{
			get { return RelativityInstanceElement.SelectedOption.Text; }
			set { RelativityInstanceElement.SelectByText(value); }
		}

		public PushToRelativitySecondPage(RemoteWebDriver driver) : base(driver)
		{
			driver.SwitchTo().Frame("configurationFrame");
			WaitForPage();
			PageFactory.InitElements(driver, this);

			FolderLocationSelect = new TreeSelect(driver.FindElement(By.XPath(@"//div[@id='location-select']/..")),
				"location-select", "jstree-holder-div");
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
			FolderLocation.ClickEx(TimeSpan.FromSeconds(timeoutInSeconds));
			return this;
		}

		public PushToRelativitySecondPage SelectProductionLocation(string productionName)
		{
			ProductionLocation.ClickEx();
			WaitForPage();
			ProductionLocationSelect.Choose(productionName);
			return this;
		}

		public PushToRelativityThirdPage GoToNextPage()
		{
			WaitForPage();

			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().Frame(_mainFrameNameOldUi);

			Thread.Sleep(TimeSpan.FromMilliseconds(200));
			NextButton.ClickEx();
			return new PushToRelativityThirdPage(Driver);
		}
	}
}