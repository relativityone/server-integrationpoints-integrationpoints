using System;
using System.Threading;
using kCura.IntegrationPoints.UITests.Components;
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

		[FindsBy(How = How.Id, Using = "saved-search-selection-button")]
		protected IWebElement SavedSearchSelectionButton { get; set; }

		[FindsBy(How = How.Id, Using = "s2id_savedSearchSelector")]
		protected IWebElement SavedSearchSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "s2id_sourceProductionSetsSelector")]
		protected IWebElement SourceProductionSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "s2id_productionSetsSelector")]
		protected IWebElement ProductionLocationSelectWebElement { get; set; }

		protected Select SavedSearchSelect => new Select(SavedSearchSelectWebElement);

		protected Select SourceProductionSelect => new Select(SourceProductionSelectWebElement);

		protected Select ProductionLocationSelect => new Select(ProductionLocationSelectWebElement);

		protected Select DestinationWorkspaceSelect { get; set; }

		protected Select RelativityInstanceSelect { get; set; }

		protected SelectElement SourceSelectElement => new SelectElement(SourceSelectWebElement);

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
			FolderLocationSelect = new TreeSelect(driver.FindElement(By.XPath(@"//div[@id='location-select']/..")));
		}

		public PushToRelativitySecondPage SelectSavedSearch()
		{
			SavedSearchSelect.Choose("All Documents");
			Sleep(200);
			return this;
		}

		public PushToRelativitySecondPage SelectSourceProduction(string productionName)
		{
			SourceProductionSelect.Choose(productionName);
			WaitForPage();
			return this;
		}

		public PushToRelativitySecondPage SelectFolderLocation()
		{
			WaitForPage();
			FolderLocation.Click();
			return this;
		}

		public PushToRelativitySecondPage SelectProductionLocation(string productionName)
		{
			ProductionLocation.Click();
			WaitForPage();
			ProductionLocationSelect.Choose(productionName);
			return this;
		}

		public PushToRelativityThirdPage GoToNextPage()
		{
			WaitForPage();

			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().Frame("externalPage");

			Thread.Sleep(TimeSpan.FromMilliseconds(200));
			NextButton.Click();
			return new PushToRelativityThirdPage(Driver);
		}
	}
}
